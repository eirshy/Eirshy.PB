using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eirshy.PB.PressXToJson.Enums {
    internal enum LoggingLevel {
        None,
        AsOrphan,

        //order here is precedence order
        ErrorOnly,
        ErrorVerbose,
        Info,
        InfoVerbose,
    }

    internal static class LoggingLevelConfig {
        public const LoggingLevel DEFAULT_PER_MOD = LoggingLevel.AsOrphan;
        public const LoggingLevel DEFAULT_ORPHAN = LoggingLevel.ErrorOnly;
        public const long DEFAULT_MAX_KB_ORPHAN = 1000;
        public const long DEFAULT_MAX_KB_MOD = 25;
        public const long DEFAULT_MAX_KB_INSF = 0;

        public static SortedDictionary<string,string> GetDocs() {
            var ret = new SortedDictionary<string, string> {
                [LoggingLevel.None.ToString()] = "No logging for this mod",
                [LoggingLevel.ErrorOnly.ToString()] = "Simplified error messages will be logged.",
                [LoggingLevel.ErrorVerbose.ToString()] = "Errors will include inner exception unrolls and some serialized objects.",
                [LoggingLevel.Info.ToString()] = "Verbose Errors and Simplified Info messages will be logged.",
                [LoggingLevel.InfoVerbose.ToString()] = "Errors and Info messages will be logged with round-trip serialization, etc.",
                [LoggingLevel.AsOrphan.ToString()] = "Log level will be synchronized with the Orphanage setting",

                ["zz PER-MOD DEFAULT"] = DEFAULT_PER_MOD.ToString(),
                ["zz ORPHAN DEFAULT"] = DEFAULT_ORPHAN.ToString(),
            };
            return ret;
        }
    }
}
