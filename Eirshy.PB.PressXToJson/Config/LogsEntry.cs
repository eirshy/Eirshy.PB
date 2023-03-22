using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Config {
    internal class LogsEntry {
        public static readonly LogsEntry DEFAULT = new LogsEntry();

        internal LoggingLevel Level { get; private set; } = LoggingLevelConfig.DEFAULT_PER_MOD;
        public string Lvl {
            get => Level.ToString();
            set => Level = Enum.TryParse<LoggingLevel>(value, out var ret) ? ret : LoggingLevelConfig.DEFAULT_PER_MOD;
        }

        internal LoggingCustody Custody { get; private set; } = LoggingCustodyConfig.DEFAULT_PER_MOD;
        public string Cstdy {
            get => Custody.ToString();
            set => Custody = Enum.TryParse<LoggingCustody>(value, out var ret) ? ret : LoggingCustodyConfig.DEFAULT_PER_MOD;
        }

    }
}
