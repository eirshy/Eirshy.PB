using System;
using System.Linq;
using System.Collections.Generic;

using PhantomBrigade.Mods;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class InstructionsFile : IComparable<InstructionsFile>, IEquatable<InstructionsFile> {
        #region Readonly/Const Defaults -- NONE
        #endregion

        #region Standard Edit Properties (Disabled, Priority, Ins, Text)

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

        #endregion
        #region Advanced Properties (...Rest) -- NONE
        #endregion

        #region Mod Owner, Load Orders, & physical source

        internal ModData Owner { get; set; }
        internal int FileLoadOrder { get; set; }
        internal string PhysicalSource { get; set; }

        #endregion
        #region Resolved Data Linking

        internal Type SourceFileType { get; set; }

        internal string SourceRoute { get; set; }
        internal string SourceFile { get; set; }

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
        internal string ShortName => $"{SourceRoute}/{SourceFile}";

        #region Shorthand Utilities -- NONE
        #endregion

        #region Interface implementation -- IComparable, IEquatable, IDisposable

        public int CompareTo(InstructionsFile other) {
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
