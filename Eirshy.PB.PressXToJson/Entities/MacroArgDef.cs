using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class MacroArgDef {
        public JTokenType Type { get; set; }
        public JToken Default { get; set; }
        public string Docs { get; set; }
    }
}
