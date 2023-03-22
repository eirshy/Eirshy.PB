using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;


namespace Eirshy.PB.PressXToJson.Fixes {
    internal static class FixesRollup {
        public static void ApplyFixes(Harmony harmony) {
            Functions.Equipment.TrimSystemsByTagFilterPatches.Apply(harmony);
        }
    }
}
