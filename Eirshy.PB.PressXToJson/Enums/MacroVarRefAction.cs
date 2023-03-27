using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eirshy.PB.PressXToJson.Enums {
    internal enum MacroVarRefAction {
        Direct,
        #region Simple Strings
        
        Text,

        #endregion
        #region Simple Objects
        
        Merge,
        MergePatch,
        MergeHard,

        #endregion
        #region Simple Arrays

        Spread,
        SpreadPatch,

        #endregion
    }
}
