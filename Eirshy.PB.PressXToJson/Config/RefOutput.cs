using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Config {
    internal class RefOutput {
        const RefOutputFreq DEFAULT_OUTPUT_FREQ = RefOutputFreq.Never;

        internal RefOutputFreq DumpVanilla { get; private set; } = DEFAULT_OUTPUT_FREQ;
        internal RefOutputFreq DumpResolved { get; private set; } = DEFAULT_OUTPUT_FREQ;

        public string Docs {
            get => $"When we should output reference files." +
                $" Values are: {string.Join(", ",Enum.GetNames(typeof(RefOutputFreq)))}." +
                $" Default is {DEFAULT_OUTPUT_FREQ}."
            ;
        }

        public string Vanilla {
            get => DumpVanilla.ToString();
            set => DumpVanilla = Enum.TryParse<RefOutputFreq>(value, out var ret) ? ret : DEFAULT_OUTPUT_FREQ;
        }
        public string Resolved {
            get => DumpResolved.ToString();
            set => DumpResolved = Enum.TryParse<RefOutputFreq>(value, out var ret) ? ret : DEFAULT_OUTPUT_FREQ;
        }
    }
}
