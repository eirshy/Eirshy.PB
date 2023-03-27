using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class MacroDef {
        public Dictionary<string, MacroArgDef> Args { get; set; }
        public List<MacroVarRef> Vars { get; set; }
        public JObject Text { get; set; }
        public JArray Ins { get; set; }


        internal InstructionsFile Owner { get; set; }
        internal Dictionary<string, JToken> VarData { get; set; }

    }
}
