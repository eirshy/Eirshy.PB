using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eirshy.PB.PressXToJson.Enums {
    internal enum LoggingCustody {
        ModRoot, PerFile, AsOrphan, 
    }

    internal static class LoggingCustodyConfig {
        public const string FILENAME_Orphanage = "json.orphan.log";
        public const string FILENAME_ModRoot = "json.mod.log";
        public const string FILENAME_PerFileSuffix = ".log";
        public const LoggingCustody DEFAULT_PER_MOD = LoggingCustody.AsOrphan;
        public const LoggingCustody ORPHANAGE = LoggingCustody.ModRoot;

        public static SortedDictionary<string, string> GetDocs() {
            var ret = new SortedDictionary<string, string> {
                [LoggingCustody.ModRoot.ToString()] = $"Custody is given to the mod, placed in a {FILENAME_ModRoot} file in the root directory of the mod.",
                [LoggingCustody.PerFile.ToString()] =
                    $"Custody prioritizes the file, palced in a {FILENAME_PerFileSuffix} file if possible, otherwise as {LoggingCustody.ModRoot}"
                ,
                [LoggingCustody.AsOrphan.ToString()] = "Custody is given to the logs orphanage.",

                ["zz PER-MOD DEFAULT"] = DEFAULT_PER_MOD.ToString(),
                ["zz ORPHANAGE"] = $"As {ORPHANAGE} for whichever mod 'owns' the orphanage.",
            };
            return ret;
        }
    }
}
