using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class LocalizationEntry {
        /// <summary>
        /// Maps to __name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Maps to __text
        /// </summary>
        public string Desc { get; set; }


        internal string Language { get; set; }
        internal string SectorKey { get; set; }
        internal string EntryRoot { get; set; }

        internal InstructionsFile Owner { get; set; }
    }
}
