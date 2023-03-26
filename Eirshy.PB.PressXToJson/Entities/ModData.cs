using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PhantomBrigade.Mods;
using PhantomBrigade.Data;

using Eirshy.PB.PressXToJson.Config;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class ModData : IComparable<ModData>, IEquatable<ModData>, IDisposable {
        static int _loadOrder = 0;
        public ModData(ModLoadedData builtin, string rootLocation) {
            ModLoadOrder = _loadOrder++;
            Builtin = builtin;
            RootLocation = rootLocation;
        }

        internal void LoadSettingsFile() {
            var target = Path.Combine(RootLocation, ModSettingsFile.FILENAME);
            if(File.Exists(target)) {
                try {
                    var raw = File.ReadAllText(target);
                    SettingsFile = raw.JsonTo<ModSettingsFile>();
                    SettingsFile.Owner = this;
                    SettingsFile.PhysicalSource = target;
                } catch(Exception ex) {
                    SettingsFile = new ModSettingsFile() { Owner = this, PhysicalSource = target, };
                    SettingsFile.AddError(ex);
                }

            } else SettingsFile = new ModSettingsFile() { Owner = this, PhysicalSource = target };
        }
        internal void LogAllErrors() {
            Files.ForEach(file => file.LogErrors());
        }


        internal ModLoadedData Builtin { get; }
        internal int ModLoadOrder { get; }
        internal string RootLocation { get; }

        internal List<InstructionsFile> Files { get; set; }
        internal ModSettingsFile SettingsFile { get; set; }

        internal LogsEntry LogSettings { get; set; }
        private Logger _log = null;
        internal Logger Log {
            get {
                if(_log is null) _log = Logger.Register(this, null);
                return _log;
            }
        }

        internal string Name => Builtin.metadata.id;

        internal IEnumerable<Instruction> Instructions
            => Files.SelectMany(file => file.Ins)
        ;
        internal string Stats => $"{Name}" +
            $" :: {Files.Where(file => !file.Disabled).Count()} / {Files.Count} Files" +
            $" :: {Instructions.Where(ins => !ins.Disabled).Count()} / {Instructions.Count()} Ins"
        ;


        #region Interface implementation -- IComparable, IEquatable, IDisposable

        public int CompareTo(ModData other) {
            var cmp = ModLoadOrder.CompareTo(other.ModLoadOrder);
            return cmp;
        }

        public bool Equals(ModData other) {
            if(other is null) return false;
            return ModLoadOrder == other.ModLoadOrder;
        }
        public override int GetHashCode() => ModLoadOrder.GetHashCode();

        /// <summary>
        /// Logs all errors and disposes the logger if it's not an Orphan one.
        /// </summary>
        public void Dispose() {
            Files.Clear();
            if(_log == null) return;
            if(Log.Custody != Enums.LoggingCustody.AsOrphan) {
                Log.Dispose();
            }
            _log = null;
        }

        #endregion

    }
}
