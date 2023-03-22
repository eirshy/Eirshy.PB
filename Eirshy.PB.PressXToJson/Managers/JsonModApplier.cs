using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using PhantomBrigade.Mods;
using PhantomBrigade.Data;

using Eirshy.PB.PressXToJson.InstructionProcessing;
using Eirshy.PB.PressXToJson.Entities;

namespace Eirshy.PB.PressXToJson.Managers {
    internal static class JsonModApplier {
        private static Type THIS => typeof(JsonModApplier);
        private static Dictionary<Type, List<Instruction>> _typeMap = null;

        public static void Init(Harmony harmony) {
            _ApplyHooks(harmony);
        }

        private static void _ApplyHooks(Harmony harmony) {
            const BindingFlags FLAGS = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
            var linker = (
                mm: typeof(ModManager).GetMethod(nameof(ModManager.ProcessConfigModsForLinker), FLAGS)
                    ?.MakeGenericMethod(typeof(DataContainerUnique))
                , post: THIS.GetMethod(nameof(PostApplyEditsLinker), FLAGS)
            );
            var multiLinker = (
                mm: typeof(ModManager).GetMethod(nameof(ModManager.ProcessConfigModsForMultiLinker), FLAGS)
                    ?.MakeGenericMethod(typeof(DataContainer))
                , post: THIS.GetMethod(nameof(PostApplyEditsMultiLinker), FLAGS)
            );

            try {
                //patching generics is awkward.
                if(linker.mm is null) throw new InvalidProgramException($"Can't Find {nameof(linker)}!");
                if(multiLinker.mm is null) throw new InvalidProgramException($"Can't Find {nameof(multiLinker)}!");
                _ = harmony.Patch(linker.mm, postfix: new HarmonyMethod(linker.post));
                _ = harmony.Patch(multiLinker.mm, postfix: new HarmonyMethod(multiLinker.post));
            } catch(Exception ex) {
                Logger.Orphan.HookError("Linker-Applies", ex);
            }
        }

        internal static void Reset() {
            _typeMap.Clear();
        }

        internal static void LoadTypeInstructions() {
            _typeMap = JsonModLoader.AllJsonMods
                .SelectMany(kvp => kvp.Value.Instructions)
                .Where(ins => !ins.Disabled)
                .GroupBy(key => key.TargetType)
                .ToDictionary(
                    tk => tk.Key,
                    tv => tv.ToList()
                )
            ;
            foreach(var insl in _typeMap.Values) {
                insl.Sort();
            }

            Logger.Orphan.Info($"Hooked {_typeMap.Count} type linkers.");
        }
        /// <remarks>
        /// See remarks on JsonModLoader.FinalizeLoadedMods()
        /// </remarks>
        internal static void ForceFinalizeLoadedMods() {
            if(_typeMap is null) {
                JsonModLoader.FinalizeLoadedMods();
            }
        }


        //"why?" see "Generics" under https://harmony.pardeike.net/articles/patching-edgecases.html

        public static void PostApplyEditsLinker(Type dataType, object dataInternal, ref object __result) {
            ForceFinalizeLoadedMods();
            if(!_typeMap.TryGetValue(dataType, out var instructions)) return;//no change
            var lnk = DataLinker.GetForType(dataType);
            lnk.Apply(instructions, dataInternal, ref __result);
        }

        public static void PostApplyEditsMultiLinker(Type dataType, object dataInternal) {
            ForceFinalizeLoadedMods();
            if(!_typeMap.TryGetValue(dataType, out var instructions)) return;//no change
            var lnk = DataLinker.GetForType(dataType);
            lnk.Apply(instructions, dataInternal);
        }
    }
}
