using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PhantomBrigade.Mods;

using Eirshy.PB.PressXToJson.Config;

namespace Eirshy.PB.PressXToJson {
    public class PressXToJson : ModLink {
        internal static readonly string DllRoot = Path.GetDirectoryName(typeof(PressXToJson).Assembly.Location).Replace('\\','/') + "/";

        internal static ConfigFile Config { get; private set; }

        public override void OnLoad(Harmony harmony) {
            var configFile = DllRoot + ConfigFile.FILENAME;
            Exception configLoadFailure = null;
            if(File.Exists(configFile)) {
                string raw;
                try {
                    raw = File.ReadAllText(configFile);
                    Config = JsonConvert.DeserializeObject<ConfigFile>(raw);
                    Config.LastLoadedText = raw;
                } catch(Exception ex) {
                    Config = new ConfigFile();
                    configLoadFailure = ex;
                }
            } else Config = new ConfigFile();
            Config.PhysicalFile = configFile;
            Logger.StartOrphanage(Config.Logs.Orphan);
            if(configLoadFailure != null) {
                Logger.Orphan.Error("Error loading Config File", configLoadFailure);
            } else {
                Managers.JsonModLoader.Init(harmony);
                Managers.JsonModApplier.Init(harmony);
                Managers.ReferenceFilesManager.Init(harmony);
                Managers.LoadStateManager.Init(harmony);

                Fixes.FixesRollup.ApplyFixes(harmony);
            }
            //if we have anything, you need to hear about it.
            Logger.Orphan.Flush();
        }
    }
}
