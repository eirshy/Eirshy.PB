using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using PhantomBrigade.Mods;
using PhantomBrigade.Data;

using Eirshy.PB.PressXToJson.Processing;
using Eirshy.PB.PressXToJson.Config;
using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Managers {
    internal static class ReferenceFilesManager {
        private static Type THIS => typeof(ReferenceFilesManager);
        const string VANILLA_FOLDER = "Vanilla";
        const string RESOLVED_FOLDER = "Resolved";
        static readonly Type ALWAYS_SKIP = typeof(DataContainerPaths);//required to get the paths
        
        static RefOutput _refOut = null;
        static readonly HashSet<Type> _vanilla = new HashSet<Type>();
        static readonly HashSet<Type> _resolved = new HashSet<Type>();
        

        public static void Init(Harmony harmony) {
            _ApplyHooks(harmony);
        }
        private static void _ApplyHooks(Harmony harmony) {
            const BindingFlags FLAGS = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
            var linkers = new[] {
                typeof(ModManager).GetMethod(nameof(ModManager.ProcessConfigModsForLinker), FLAGS)
                    ?.MakeGenericMethod(typeof(DataContainerUnique))
                , typeof(ModManager).GetMethod(nameof(ModManager.ProcessConfigModsForMultiLinker), FLAGS)
                    ?.MakeGenericMethod(typeof(DataContainer))
            };
            var multiLinker = typeof(ModManager).GetMethod(nameof(ModManager.ProcessConfigModsForMultiLinker), FLAGS)
                ?.MakeGenericMethod(typeof(DataContainer))
            ;
            var vanilla = THIS.GetMethod(nameof(Vanilla), FLAGS);
            var resolved = THIS.GetMethod(nameof(Resolved), FLAGS);
            // JsonModApplier already warns if we can't find a linker, so just silent here
            
            _refOut = PressXToJson.Config.RefOutput;
            linkers.ForEach(lnk => {
                if(lnk is null) return;
                HarmonyMethod pre = _refOut.DumpVanilla != RefOutputFreq.Never ? new HarmonyMethod(vanilla) : null;
                HarmonyMethod post = _refOut.DumpResolved != RefOutputFreq.Never ? new HarmonyMethod(resolved) : null;
                try {
                    _ = harmony.Patch(lnk, prefix: pre, postfix: post);
                } catch(Exception ex) {
                    Logger.Orphan.HookError("References", ex);
                }
            });
        }

        public static void Reset() {
            _vanilla.Clear();
            _resolved.Clear();
        }

        public static void Vanilla(Type dataType, object dataInternal) {
            if(!_vanilla.Add(dataType) || dataType == ALWAYS_SKIP) return;
            var lnk = DataLinker.GetForType(dataType);
            var path = Path.Combine(PressXToJson.DllRoot, VANILLA_FOLDER, DataPathUtility.GetPath(dataType));
            lnk.WriteReferences(path, _refOut.DumpVanilla == RefOutputFreq.Missing, dataInternal);
        }
        public static void Resolved(Type dataType, object dataInternal) {
            if(!_vanilla.Add(dataType) || dataType == ALWAYS_SKIP) return;
            var lnk = DataLinker.GetForType(dataType);
            var path = Path.Combine(PressXToJson.DllRoot, RESOLVED_FOLDER, DataPathUtility.GetPath(dataType));
            lnk.WriteReferences(path, _refOut.DumpResolved == RefOutputFreq.Missing, dataInternal);
        }
    }
}
