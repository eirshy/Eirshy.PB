using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Entities;
using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson {
    /// <remarks>
    /// The dispose removes us from our internal tracking and the static ALL-FLUSH provider.
    /// <br />We're Thread-durable, not quite thread-safe, unforutnately....
    /// <br />
    /// But there's not a lot of parallelization anyways so that's Probably Fine, and we're
    /// Thread Safe is all contexts we introduced.
    /// </remarks>
    internal class Logger : IDisposable {
        static string DEFAULT_FILE => Path.Combine(PressXToJson.DllRoot, LoggingCustodyConfig.FILENAME_Orphanage);
        static readonly ConcurrentDictionary<string, Logger> _adopted = new ConcurrentDictionary<string, Logger>();


        public static Logger Orphan { get; private set; } = null;
        private static string _getOrphanKeyFor(LoggingLevel level) => $"_PRESS_X_LOG_ORPHAN__{level}";

        public string Name { get; private set; }
        public LoggingLevel Level { get; private set; }
        public LoggingCustody Custody { get; private set; }
        public bool Buffered => true;

        private string _logFile = null;
        public string LogFile => _logFile ?? Orphan?._logFile ?? DEFAULT_FILE;//safety, we coalesce to this.

        bool _neverWrote = true;

        public static Logger Register(ModSettings mod, InstructionsFile insf) {
            var le = mod.LogSettings;
            if(le.Level == LoggingLevel.AsOrphan && le.Custody == LoggingCustody.AsOrphan) {
                return Orphan;//we are the orphan!
            }
            string file;
            string name;
            long maxSize = -1;//disables this rule
            switch(le.Custody) {
                case LoggingCustody.AsOrphan:
                    name = _getOrphanKeyFor(le.Level);
                    file = null;
                    break;
                case LoggingCustody.PerFile:
                    if(insf is null) goto case LoggingCustody.ModRoot;
                    name = insf.Name;
                    file = insf.PhysicalSource + LoggingCustodyConfig.FILENAME_PerFileSuffix;
                    maxSize = PressXToJson.Config.Logs.MaxKB_PerFileLog;
                    break;
                case LoggingCustody.ModRoot:
                    name = mod.Name;
                    file = Path.Combine(mod.RootLocation, LoggingCustodyConfig.FILENAME_ModRoot);
                    maxSize = PressXToJson.Config.Logs.MaxKB_PerModLog;
                    break;
                default:
                    Orphan.Error($"Mod {mod.Name} has an unknown log custody value: {le.Custody}");
                    return Orphan;//orphaned ourselves lol
            }
            var fresh = new Logger{
                _logFile = file,
                Level = le.Level,
                Custody = le.Custody,
                Name = name,
            };
            var ret = _adopted.GetOrAdd(name, fresh);
            if(ret == fresh && file != null) {
                ret.EnforceFilesize(maxSize);
            }
            return ret;
        }
        public static void StartOrphanage(LoggingLevel level) {
            if(Orphan != null) return;
            if(level == LoggingLevel.AsOrphan) level = LoggingLevelConfig.DEFAULT_ORPHAN;
            Orphan = new Logger() {
                Level = level,
                Custody = LoggingCustody.AsOrphan,
            };
        }
        public static void TransferOrphanage() {
            if(Orphan._logFile != null) return;
            var owner = PressXToJson.Config.Logs.OrphanageOwner;

            if(Managers.LoadingManager.AllJsonMods.TryGetValue(owner, out var mod)) {
                Orphan._logFile = Path.Combine(mod.RootLocation, LoggingCustodyConfig.FILENAME_Orphanage);
                Orphan.EnforceFilesize(PressXToJson.Config.Logs.MaxKB_OrphanLog);
            } else {
                Orphan._logFile = DEFAULT_FILE;
                if(Orphan._neverWrote) {
                    //if we've written before here, there's catastrophic errors in that file
                    //  ignore the limits, we probably need that.
                    Orphan.EnforceFilesize(PressXToJson.Config.Logs.MaxKB_OrphanLog);
                }
            }
        }
        public static void SellOrphanage() {
            Orphan.Flush();
            Orphan._logFile = null;
        }
        

        readonly Lazy<ConcurrentQueue<string>> _msg = new Lazy<ConcurrentQueue<string>>(
            ()=>new ConcurrentQueue<string>(), LazyThreadSafetyMode.PublicationOnly
        );

        void _log(string msg) {
            if(Buffered) {
                _msg.Value.Enqueue(msg);
            } else {
                File.AppendAllText(LogFile, "\n"+msg);
            }
        }

        int _isFlushing = 0;
        public void Flush() {
            if(Buffered) {
                if(!_msg.IsValueCreated || _msg.Value.Count == 0) return;
                if(Interlocked.Exchange(ref _isFlushing, -1) != 0) return;//someone else is already
                var toOut = new List<string>(_msg.Value.Count +5);
                while(_msg.Value.TryDequeue(out var msg)) {
                    toOut.Add(msg);
                }
                _isFlushing = 0;
                File.AppendAllLines(LogFile, toOut);
                _neverWrote = false;
            }// else { noop }
        }

        public long GetFileSizeKB() {
            if(!File.Exists(LogFile)) return 0;
            try {
                return (new FileInfo(LogFile)).Length >> 10;
            } catch {
                return 0;
            }
        }
        public void ClearFile() {
            if(File.Exists(LogFile)) {
                File.WriteAllText(LogFile, "");
            }
        }
        public void EnforceFilesize(long maxSize_KB) {
            if(maxSize_KB > 0) {
                if(GetFileSizeKB() > maxSize_KB) ClearFile();
            } else if(maxSize_KB == 0) {
                ClearFile();
            }
        }


        /// <summary>
        /// Includes pushing error message logs out as well.
        /// </summary>
        /// <param name="force">If true, we'll ignore whether LoadStateManager says init is done.</param>
        /// <param name="alsoDeregister">If true we'll also dregister all non-Orphan loggers. Don't normally do this. Thread-unsafe.</param>
        public static void FullFlush(bool force = false, bool alsoDeregister = false) {
            if(!force && !Managers.LoadStateManager.IsInitFinished) return;
            Managers.LoadingManager.LogAllErrors();
            var adopted = _adopted.Values.ToList();
            if(alsoDeregister) _adopted.Clear();
            foreach(var l in adopted) {
                try {
                    l.Flush();
                } catch(Exception ex) {
                    Orphan.Error($"Flush error on Logger {l.Name}", ex);
                }
            }
            Orphan.Flush();
        }

        public void Error(string message, Exception ex = null) {
            switch(Level) {
                case LoggingLevel.ErrorOnly:
                case LoggingLevel.Info:
                    if(ex != null) {
                        _log($"{message} -- {ex.GetType().Name} :: {ex?.Message ?? ""}");
                    } else _log(message);
                    break;

                case LoggingLevel.ErrorVerbose:
                case LoggingLevel.InfoVerbose:
                    if(ex != null) {
                        var msg = $"{message} -- {ex.GetType().Name} :: {ex?.Message ?? ""}";
                        while(ex.InnerException != null) {
                            ex = ex.InnerException;
                            msg += $"\n  :: Inner : {ex.GetType().Name} :: {ex?.Message ?? ""}";
                        }
                        _log(msg);
                    } else _log(message);
                    break;
            }
        }
        public void Info(string message) {
            switch(Level) {
                case LoggingLevel.Info:
                case LoggingLevel.InfoVerbose:
                    _log(message);
                    break;
            }
        }
        public void InfoVerboseDeserialize(string message, object jsonifyOnVerbose) {
            switch(Level) {
                case LoggingLevel.Info:
                    _log(message);
                    break;
                case LoggingLevel.InfoVerbose:
                    _log($"{message} -- Deserialized: {jsonifyOnVerbose.ToJson()}");
                    break;
            }
        }



        public void HookError(string context, Exception ex = null) => Error($"Hook failed -- {context}", ex);
        public void ConfigLoadError(Exception ex) => Error("Error loading Config File", ex);

        public void ReferenceFileError(Type type, string name, Exception ex = null) {
            if(name is null) {
                Error($"{type.Name} @ -- Reference file failed", ex);
            } else {
                Error($"{type.Name} @ {name} -- Reference file failed", ex);
            }
        }

        public void DidOverwrite(Instruction ins) {
            switch(Level) {
                case LoggingLevel.Info:
                case LoggingLevel.InfoVerbose:
                    _log($"{ins.TargetType.Name} @ {ins.TargetName} -- Overwritten by {ins.Name}");
                    break;
            }
        }


        public void DeserializeError(string name, Type target, JObject jobj, Exception ex) {
            switch(Level) {
                case LoggingLevel.ErrorVerbose:
                    try {
                        Error($"Failed deserialize of {name} @ {target.Name} from {jobj.ToRawJson()}", ex);
                    } catch(Exception ex2) {
                        Error($"Failed verbose serialize of {name} @ {target.Name}", ex2);
                        goto case LoggingLevel.Info;//
                    }
                    break;
                case LoggingLevel.Info:
                    Error($"{target.Name} @ {name} -- Failed Deserialize", ex);
                    break;
                case LoggingLevel.InfoVerbose:
                    //info already logged jobj
                    Error($"{target.Name} @ {name} -- Failed Deserialize", ex);
                    break;
            }
        }
        public void DeserializeSuccess(string name, object from) {
            switch(Level) {
                case LoggingLevel.Info:
                    _log($"{from.GetType().Name} @ {name} -- Deserialized!");
                    break;
                case LoggingLevel.InfoVerbose:
                    _log($"{from.GetType().Name} @ {name} -- Deserialized! Round-Trip: {from.ToJson()}");
                    break;
            }
        }


        public void Dispose() {
            if(_adopted.TryGetValue(Name, out var named) && named == this) {
                if(_adopted.TryRemove(Name, out var rm) && rm != this) {
                    if(!_adopted.TryAdd(Name, rm)) {
                        _ = _adopted.TryAdd(Name + _adopted.Count, rm);
                        //best I can do; MS really fucked up in not having an rm-if-refeq
                    }
                }
            }
            try { Flush(); } catch { }
        }
    }
}
