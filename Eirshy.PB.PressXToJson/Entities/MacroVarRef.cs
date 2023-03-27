using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class MacroVarRef {
        [JsonProperty(propertyName:"@VAR")]
        public string VarName { get; set; }
        /// <summary>
        /// For use in VarDefs only. Probably.
        /// </summary>
        public string FromName { get; set; }

        public MacroVarRefAction Action { get; set; } = MacroVarRefAction.Direct;
        #region All Arguments 
        // All load, but only those that are real for the type are used
        // The other option would be to much about with @TYPE/$type or manual parsing with JTokens
        //  which would have been kinda cancer, so nah.

        /// <summary>
        /// Used by: Text
        /// </summary>
        public string Before { get; set; }
        /// <summary>
        /// Used by: Text
        /// </summary>
        public string After { get; set; }

        #endregion
    }
}
