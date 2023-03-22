using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Config {
    internal class ConfigFile {
        public const string FILENAME = "config.json";

        public string Docs_LessLazyLoading {
            get => $"PLACEHOLDER! CURRENTLY IGNORED!" +
                $" If set, we'll force-load all of the types that we know we've changed." +
                $" This allows for all syntax errors to be logged on game start, as well as" +
                $" allows us to clean up all of our internal state tracking, as we'll have" +
                $" applied all things we do."
            ;
        }
        public bool LessLazyLoading { get => false; }


        public RefOutput RefOutput { get; set; } = new RefOutput();
        public Logs Logs { get; set; } = new Logs();


        public string Docs_ResettingValues {
            get => $"Note that you can reset any value to the default by deleting it.";
        }

        internal string PhysicalFile { get; set; }
        /// <summary>
        /// Set null to disable sync.
        /// </summary>
        internal string LastLoadedText { private get; set; } = "";
        public void SyncConfig() {
            if(LastLoadedText is null) return;//sync is disabled.
            Logs.SyncModEntries();
            var updateTo = this.ToJson();
            if(updateTo != LastLoadedText) {
                File.WriteAllText(PhysicalFile, updateTo);
            }
            LastLoadedText = null;
        }
    }
}
