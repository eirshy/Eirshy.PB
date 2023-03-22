using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PhantomBrigade.Mods;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class Instruction : IComparable<Instruction> {
        #region Readonly/Const Defaults

        static readonly string DEFAULT_CompositeNamespace =
            $"{nameof(PhantomBrigade)}.{nameof(PhantomBrigade.Functions)}.{nameof(PhantomBrigade.Functions.Equipment)}."
        ;
        static readonly string DEFAULT_CompositeAssembly = typeof(ModManager).Assembly.GetName().Name;

        #endregion

        #region Basic Properties (Disabled, Command, Target, Data)

        /// <summary>
        /// If set, we'll no-op this action. Useful for dev work and pseudo-settings.
        /// <Br />Also used internally to bail out any instruction with an error on it.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Action to do
        /// </summary>
        public Command Command { get; set; } = Command.Unknown;

        /// <summary>
        /// JPath describing the target(s) of this action
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Data value to use with the given command.
        /// </summary>
        public JToken Data { get; set; }

        #endregion
        #region Advanced Properties (...rest)

        /// <summary>
        /// If set, we'll pretend we're pathed from here to this location & filename
        /// <br />Keep in mind, we order commands by Priority, then physical file path, then index order.
        /// </summary>
        public string AsFile { get; set; }

        public bool FirstMatchOnly { get; set; } = false;

        /// <summary>
        /// The "default" namespace value for @TYPE translating.
        /// Must end with a dot ("<c>.</c>").
        /// <br />Default is <c>PhantomBrigade.Functions.Equipment.</c>
        /// </summary>
        public string CompositeNamespace { get => _compositeNamespace ?? DEFAULT_CompositeNamespace; set => _compositeNamespace = value; }
        string _compositeNamespace = null;

        /// <summary>
        /// The "default" assembly value for @TYPE translating.
        /// <br />Default is the PhantomBrigade main assembly.
        /// </summary>
        public string CompositeAssembly { get => _compositeAssembly ?? DEFAULT_CompositeAssembly; set => _compositeAssembly = value; }
        string _compositeAssembly = null;

        #endregion

        #region File Owner & Index

        internal InstructionsFile Owner { get; set; }
        internal int SourceIndex { get; set; }

        #endregion
        #region Resolved Data Linking

        internal Type TargetType { get => _type ?? Owner.SourceFileType; set => _type = value; }
        Type _type = null;

        internal string TargetName { get => _targetName ?? Owner.SourceFile; set => _targetName = value; }
        string _targetName = null;

        internal string TargetRoute { get => _targetRoute ?? Owner.SourceRoute; set => _targetRoute = value; }
        string _targetRoute = null;

        #endregion
        #region Exception Spooling

        private List<Exception> _errors = new List<Exception>();
        /// <summary>
        /// Adds the passed exception to our error spool, and disables us.
        /// <br />NOT THREADSAFE
        /// </summary>
        internal void AddError(Exception ex) {
            Disabled = true;
            _errors.Add(ex);
        }
        /// <summary>
        /// Unloads our error spool into the logger.
        /// </summary>
        internal void LogErrors() {
            var flushing = _errors;
            if(flushing.Count == 0) return;
            _errors = new List<Exception>();
            var name =  Log.Custody == LoggingCustody.PerFile ? ShortName: Name;
            foreach(var ex in flushing) {
                Log.Error(name, ex);
            }
        }

        #endregion
        #region Logging Source

        internal Logger Log => Owner.Log;//instructions don't get their own

        #endregion

        internal string Name => $"{Owner.Name}::{ShortName}";
        internal string ShortName => $"ins[{SourceIndex}]";

        #region Shorthand Utilities

        internal JArray DataArr => Data as JArray;
        internal JObject DataObj => Data as JObject;
        internal string DataStr => Data?.Type == JTokenType.String ? Data.Value<string>() : null;

        #endregion

        #region Interface implementation -- IComparable

        public int CompareTo(Instruction other) {
            var cmp = Command.ComparePriority(other.Command);
            if(cmp != 0) return cmp;
            if(Owner != other.Owner) {
                return Owner.CompareTo(other.Owner);
            } else return SourceIndex.CompareTo(other.SourceIndex);
        }
        
        #endregion
    }
}
