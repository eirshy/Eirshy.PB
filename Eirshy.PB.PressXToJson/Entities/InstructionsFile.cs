using System;
using System.Linq;
using System.Collections.Generic;

using PhantomBrigade.Mods;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class InstructionsFile : IComparable<InstructionsFile>, IEquatable<InstructionsFile> {
        #region Readonly/Const Defaults

        //Instruction
        static readonly string DEFAULT_InstructionNamespace =
            $"{nameof(PhantomBrigade)}.{nameof(PhantomBrigade.Data)}."
        ;
        static readonly string DEFAULT_InstructionAssembly = typeof(ModManager).Assembly.GetName().Name;

        //Composite
        static readonly string DEFAULT_CompositeNamespace =
            $"{nameof(PhantomBrigade)}.{nameof(PhantomBrigade.Functions)}.{nameof(PhantomBrigade.Functions.Equipment)}."
        ;
        static readonly string DEFAULT_CompositeAssembly = typeof(ModManager).Assembly.GetName().Name;


        #endregion

        #region Standard Edit Properties (Disabled, Priority, Ins, Text, Macro)

        /// <summary>
        /// If set, this file will be completely ignored outside of initial parsing errors.
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// A means to override the file load order.
        /// <br />Lower number files go off before higher number files.
        /// <br />Default value is 1000
        /// </summary>
        public int Priority { get; set; } = 1000;

        public IList<Instruction> Ins { get; set; }


        /// <summary>
        /// <c>this [Filename] [KeyRoot] [Language] -&gt; LanguageEntry</c>
        /// <br />Yes this is kinda ridiculous lol
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, LocalizationEntry>>> Text { get; set; }

        public Dictionary<string, MacroDef> Macro { get; set; }

        #endregion
        #region Advanced Properties (...Rest)

        /// <summary>
        /// If set, we'll use this as our File's Type, instead of the file path.
        /// <br />Same rules as @TYPE just with <c>PhantomBrigade.Data</c> as the default namespace.
        /// </summary>
        public string AsType { get; set; } = null;
        public string AsName { get; set; } = null;


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


        /// <summary>
        /// The "default" namespace value for @TYPE translating.
        /// Must end with a dot ("<c>.</c>").
        /// <br />Default is <c>PhantomBrigade.Functions.Equipment.</c>
        /// </summary>
        public string InstructionNamespace { get => _instructionNamespace ?? DEFAULT_InstructionNamespace; set => _instructionNamespace = value; }
        string _instructionNamespace = null;

        /// <summary>
        /// The "default" assembly value for @TYPE translating.
        /// <br />Default is the PhantomBrigade main assembly.
        /// </summary>
        public string InstructionAssembly { get => _instructionAssembly ?? DEFAULT_InstructionAssembly; set => _instructionAssembly = value; }
        string _instructionAssembly = null;

        #endregion

        #region Mod Owner, Load Orders, & physical source

        internal ModData Owner { get; set; }
        internal int FileLoadOrder { get; set; }
        internal string PhysicalSource { get; set; }

        #endregion
        #region Resolved Data Linking

        internal Type AmbientType { get; set; }

        internal string SourceRoute { get; set; }
        internal string AmbientName { get; set; }

        #endregion
        #region Exception Spooling

        private List<Exception> _errors = new List<Exception>();
        /// <summary>
        /// Adds the passed exception to our error spool, and disables us.
        /// <br />NOT THREADSAFE but probably should be lol
        /// </summary>
        internal void AddError(Exception ex) {
            Disabled = true;
            _errors.Add(ex);
        }
        /// <summary>
        /// Unloads our error spool into the logger.
        /// </summary>
        internal void LogErrors() {
            if(_errors.Count > 0) {
                var flushing = _errors;
                _errors = new List<Exception>();
                var name =  Log.Custody == LoggingCustody.PerFile ? ShortName: Name;
                foreach(var ex in flushing) {
                    Log.Error(name, ex);
                }
            }
            foreach(var ins in Ins) {
                ins.LogErrors();
            }
        }

        #endregion
        #region Logging Source

        private Logger _log = null;
        internal Logger Log {
            get {
                if(_log is null) _log = Logger.Register(Owner, this);
                return _log;
            }
        }

        #endregion

        internal string Name => $"{Owner.Name}::{ShortName}";
        internal string ShortName => $"{SourceRoute}/{AmbientName}";

        #region Shorthand Utilities -- NONE
        #endregion

        #region Interface implementation -- IComparable, IEquatable, IDisposable

        public int CompareTo(InstructionsFile other) {
            if(ReferenceEquals(this, other)) return 0;
            var cmp = Owner.CompareTo(other.Owner);
            if(cmp != 0) return cmp;
            cmp = Priority.CompareTo(other.Priority);
            if(cmp != 0) return cmp;
            cmp = FileLoadOrder.CompareTo(other.FileLoadOrder);
            return cmp;
        }

        public bool Equals(InstructionsFile other) {
            if(other == null) return false;
            return Owner.Equals(other.Owner)
                && FileLoadOrder == other.FileLoadOrder
            ;
        }
        public override int GetHashCode()
            => Owner.GetHashCode()
            ^ FileLoadOrder.GetHashCode()
        ;

        /// <summary>
        /// Logs all errors and disposes the logger if it's a File logger
        /// </summary>
        public void Dispose() {
            //there's no way to shortcut this smartly.
            LogErrors();
            if(Log.Custody == LoggingCustody.PerFile) {
                Log.Dispose();
            }
        }

        #endregion

    }
}
