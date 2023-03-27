using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;

using Newtonsoft.Json.Linq;

using PhantomBrigade.Mods;
using PhantomBrigade.Data;
using PhantomBrigade.Functions.Equipment;

using Eirshy.PB.PressXToJson.Entities;
using Eirshy.PB.PressXToJson.Enums;
using Eirshy.PB.PressXToJson.Exceptions;

namespace Eirshy.PB.PressXToJson.Processing {
    internal abstract class DataLinker {
        private static readonly ConcurrentDictionary<Type, DataLinker> _linkers = new ConcurrentDictionary<Type, DataLinker>();

        public static DataLinker GetForType(Type type) {
            if(_linkers.TryGetValue(type, out var ret)) return ret;
            return _linkers.GetOrAdd(type, (DataLinker)typeof(DataLinker<>).MakeGenericType(type).GetConstructor(Type.EmptyTypes).Invoke(null));
        }
        #region Cache-clearing
        
        public static void ClearLinkerCache() => _linkers.Clear();

        #endregion
        #region Abstract Boxed Apply Instructions

        public abstract void Apply(List<Instruction> instructions, object dataInternal, ref object __result);
        public abstract void Apply(List<Instruction> instructions, object dataInternal);
        
        #endregion
        #region Abstract Boxed Write References
        
        public abstract void WriteReferences(string path, bool missingOnly, object dataInternal);
        
        #endregion

    }

    internal class DataLinker<T> : DataLinker where T : class, new() {
        public DataLinker() { }
        #region Apply Instructions

        public override void Apply(List<Instruction> instructions, object dataInternal, ref object __result) 
            => Apply(instructions, (T)dataInternal, ref __result)
        ;
        public override void Apply(List<Instruction> instructions, object dataInternal)
            => Apply(instructions, (SortedDictionary<string, T>)dataInternal)
        ;

        public void Apply(List<Instruction> instructions, T dataInternal, ref object __result) {
            var target = (__result ?? dataInternal ?? new T()).ToJObject();
            var pathproc = new PathedCommandProcessor();
            var loggers = new HashSet<Logger>();
            foreach(var ins in instructions) {
                if(ins.Disabled) continue;
                try {
                    _ = loggers.Add(ins.Log);
                    var updated = PathlessApply(ins, target, null);
                    if(updated is null) target = pathproc.Apply(ins, target);
                    else target = updated;
                } catch(Exception ex) {
                    ins.AddError(ex);
                }
            }
            try {
                var ret = target.ToType<T>();
                loggers.LogForSet(logger => logger.DeserializeSuccess("Singleton", ret));
                __result = ret;
            } catch (Exception ex) {
                loggers.LogForSet(logger => logger.DeserializeError("Singleton", typeof(T), target, ex));
            }
            Logger.FullFlush();
        }

        public void Apply(List<Instruction> instructions, SortedDictionary<string, T> dataInternal) {
            var cache = new JObjectCache<T>(dataInternal);
            Logger.Orphan.Info($"Applying {instructions.Count} ins to type {typeof(T).Name}");
            var pathproc = new PathedCommandProcessor();
            foreach(var ins in instructions) {
                if(ins.Disabled) continue;
                var target = cache.Get(ins.TargetName, ins.Log);
                try {
                    var updated = PathlessApply(ins, target, cache);
                    if(updated is null) updated = pathproc.Apply(ins, target);
                    if(!ReferenceEquals(updated, target)) {
                        cache.Set(ins.TargetName, updated, ins.Log);
                    }
                } catch(Exception ex) {
                    ins.AddError(ex);
                }
            }
            cache.Save();
            Logger.FullFlush();
        }


        /// <summary>Interprets New and Copy commands</summary>
        /// <param name="cache">If null, will be assumed unavailable.</param>
        protected JObject PathlessApply(Instruction ins, JObject target, JObjectCache<T> cache) {
            switch(ins.Command) {
                //Object-Typed .. ... ... ... ... ... ... ... ...
                #region New & Overwrite . ... ... ... ... ... ...

                case Command.New:
                    if(target != null) ins.Log.DidOverwrite(ins);
                    return ins.DataObj;

                case Command.Overwrite:
                    //we don't need to log that we overwrote, that's expected.
                    return ins.DataObj;

                #endregion

                //String-Typed .. ... ... ... ... ... ... ... ...
                #region PriorityCopy & Copy . ... ... ... ... ...

                case Command.CopyBase:
                case Command.Copy: {
                    if(cache is null) {//safety; should have bailed already on file loading.
                        throw new CommandTargetTypeException(ins.Command, typeof(DataContainerUnique));
                    } else if(cache.TryGetValue(ins.DataStr, out var copyFrom)) {
                        if(target != null) ins.Log.DidOverwrite(ins);
                        return (JObject)copyFrom.DeepClone();
                    } else throw new CommandException(ins.Command, $"Target {ins.DataStr} Not Found");
                }

                #endregion

                //Pathed Instructions
                default: return null;
            }
        }

        #endregion
        #region Write References

        public override void WriteReferences(string path, bool missingOnly, object dataInternal) {
            if(dataInternal is T typed) {
                WriteReference(path + ".json", missingOnly, typed);
            } else if (dataInternal is SortedDictionary<string, T> dict){
                WriteReferencesMulti(path, missingOnly, dict);
            } else {
                Logger.Orphan.Error($"Cannot write reference file for {typeof(T).Name} -- dataInternal presented as {dataInternal} of type {dataInternal?.GetType().Name}");
                Logger.FullFlush();
            }
        }

        public void WriteReference(string path, bool missingOnly, T dataInternal) {
            if(File.Exists(path) && missingOnly) return;
            try {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, dataInternal.ToJson());
            }catch(Exception ex) {
                Logger.Orphan.ReferenceFileError(typeof(T), "Singleton", ex);
            }
        }
        public void WriteReferencesMulti(string root, bool missingOnly, SortedDictionary<string, T> dataInternal) {
            try {
                _ = Directory.CreateDirectory(root);
            } catch(Exception ex) {
                Logger.Orphan.ReferenceFileError(typeof(T), "_Directories", ex);
            }
            foreach(var kvp in dataInternal) {
                try {
                    var path = Path.Combine(root, kvp.Key) + ".json";
                    if(File.Exists(path) && missingOnly) continue;
                    Logger.Orphan.Info($"Attempting {typeof(T).Name} @ '{kvp.Key}'");
                    File.WriteAllText(path, kvp.Value.ToJson());
                } catch(Exception ex) {
                    Logger.Orphan.ReferenceFileError(typeof(T), kvp.Key, ex);
                }
            }
        }

        #endregion
    }
}
