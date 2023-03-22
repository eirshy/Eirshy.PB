using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using HarmonyLib;

using PhantomBrigade.Functions.Equipment;

namespace Eirshy.PB.PressXToJson.Fixes.Functions.Equipment {
    internal static class TrimSystemsByTagFilterPatches {
        public static void Apply(Harmony harmony) {
            var run = typeof(TrimSystemsByTagFilter).GetMethod(nameof(TrimSystemsByTagFilter.Run), BindingFlags.Public|BindingFlags.Instance);
            if(run is null) {
                Logger.Orphan.HookError("Trim-By-Tags' Run method not found");
                return;
            }
            var fix = typeof(TrimSystemsByTagFilterPatches).GetMethod(nameof(FixEmptyStringOnRun), BindingFlags.Public|BindingFlags.Static);

            _ = harmony.Patch(run, prefix: new HarmonyMethod(fix));

        }

        public static void FixEmptyStringOnRun(ref TrimSystemsByTagFilter __instance) {
            _ = __instance.filter.Remove("");
        }
    }
}
