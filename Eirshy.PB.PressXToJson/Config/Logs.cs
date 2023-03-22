using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PhantomBrigade.Mods;

using Eirshy.PB.PressXToJson.Enums;
using Eirshy.PB.PressXToJson.Managers;

namespace Eirshy.PB.PressXToJson.Config {
    internal class Logs {
        internal static string DEFAULT_ORPHANAGE => PressXToJson.instance.metadata.id;

        public string Docs_This {
            get => $"By-Mod-ID config for logs." +
                $" Orphaned logs (like those from finalizing datum after multiple mods have modified it) are handled by" +
                $" the orphan settings. Any non-json mod-id attempting to foster logs will be ignored and will remain in" +
                $" the custody of the mod '{DEFAULT_ORPHANAGE}'.";
        }
        public string Docs_Size {
            get => $"On game start, if the given file is over its configured KB, it'll be cleared instead of appended to." +
                $" A value of -1 disables the auto-clear, and a value of 0 clears the file every time.";
        }
        public SortedDictionary<string,string> Docs_Level {
            get => LoggingLevelConfig.GetDocs();
        }
        public SortedDictionary<string, string> Docs_Custody {
            get => LoggingCustodyConfig.GetDocs();
        }

        public bool AddActiveJsonMods { get; set; } = true;
        public bool RemoveInactiveJsonMods { get; set; } = true;

        public long MaxKB_OrphanLog { get; set; } = LoggingLevelConfig.DEFAULT_MAX_KB_ORPHAN;
        public long MaxKB_PerModLog { get; set; } = LoggingLevelConfig.DEFAULT_MAX_KB_MOD;
        public long MaxKB_PerFileLog { get; set; } = LoggingLevelConfig.DEFAULT_MAX_KB_INSF;

        internal LoggingLevel Orphan { get; private set; } = LoggingLevelConfig.DEFAULT_ORPHAN;
        
        public string OrphanageOwner { get; set; } = DEFAULT_ORPHANAGE;
        public string OrphanLevel {
            get => Orphan.ToString();
            set {
                Orphan = Enum.TryParse<LoggingLevel>(value, out var ret) ? ret : LoggingLevelConfig.DEFAULT_ORPHAN;
                if(Orphan == LoggingLevel.AsOrphan) Orphan = LoggingLevelConfig.DEFAULT_ORPHAN;
            }
        }

        public SortedDictionary<string, LogsEntry> Mods { get; set; } = new SortedDictionary<string, LogsEntry>();

        /// <summary>
        /// Applies AddActive and RemoveDisabled
        /// </summary>
        internal void SyncModEntries() {
            try {
                if(AddActiveJsonMods) {
                    foreach(var mod in JsonModLoader.AllJsonMods.Keys) {
                        if(!Mods.ContainsKey(mod)) {
                            Mods.Add(mod, new LogsEntry());
                        }
                    }
                }
                if(RemoveInactiveJsonMods) {
                    var keep = JsonModLoader.AllJsonMods;
                    Mods.Keys
                        .Where(mod => !keep.ContainsKey(mod))
                        .ToList()//prevents moving-target error
                        .ForEach(Mods.Remove)
                    ;
                }
            } catch(Exception ex) {
                Logger.Orphan.Error("Error in ModID Log Setting Adjustments", ex);
            }
        }
    }
}
