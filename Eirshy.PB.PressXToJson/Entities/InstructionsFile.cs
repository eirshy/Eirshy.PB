﻿using System;
using System.Linq;
using System.Collections.Generic;

using PhantomBrigade.Mods;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class InstructionsFile : IComparable<InstructionsFile> {
        #region Readonly/Const Defaults -- NONE
        #endregion

        #region Basic Properties (Disabled, Opt, Ins, Priority)

        /// <summary>
        /// If set, this file will be completely ignored outside of initial parsing errors.
        /// </summary>
        public bool Disabled { get; set; }

        public IList<Instruction> Ins { get; set; }

        /// <summary>
        /// A means to override the file load order.
        /// <br />Lower number files go off before higher number files.
        /// <br />Default value is 1000
        /// </summary>
        public int Priority { get; set; } = 1000;

        #endregion
        #region Advanced Properties (...rest) -- NONE
        #endregion

        #region Mod Owner, Load Orders, & physical source

        internal ModSettings Owner { get; set; }
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
            var flushing = _errors;
            if(flushing.Count == 0) return;
            _errors = new List<Exception>();
            var name =  Log.Custody == LoggingCustody.PerFile ? ShortName: Name;
            foreach(var ex in flushing) {
                Log.Error(name, ex);
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

        #region Interface implementation -- IComparable

        public int CompareTo(InstructionsFile other) {
            var cmp = Owner.CompareTo(other.Owner);
            if(cmp != 0) return cmp;
            cmp = Priority.CompareTo(other.Priority);
            if(cmp != 0) return cmp;
            cmp = FileLoadOrder.CompareTo(other.FileLoadOrder);
            return cmp;
        }

        #endregion

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
    }
}
