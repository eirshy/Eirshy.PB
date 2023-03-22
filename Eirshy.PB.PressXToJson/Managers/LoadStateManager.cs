using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using PhantomBrigade;
using PhantomBrigade.Mods;
using PhantomBrigade.Game.Systems;

using Eirshy.PB.PressXToJson.DataProcessing;

namespace Eirshy.PB.PressXToJson.Managers {
    internal static class LoadStateManager {
        private static Type THIS => typeof(LoadStateManager);
        public static bool IsInitFinished { get; private set; } = false;

        public static void Init(Harmony harmony) {
            _ApplyHooks(harmony);
        }

        private static void _ApplyHooks(Harmony harmony) {
            const BindingFlags FLAGS = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
            var loadMods = (
                orig: typeof(ModManager).GetMethod(nameof(ModManager.LoadMods), FLAGS),
                pre: THIS.GetMethod(nameof(FullReset), FLAGS)
            );
            var linkerInit = (
                orig: typeof(DataLinkerInitSystem).GetMethod(nameof(DataLinkerInitSystem.Initialize), BindingFlags.Instance|BindingFlags.Public),
                post: THIS.GetMethod(nameof(OnInitFinished), FLAGS)
            );

            try {
                if(loadMods.orig is null) throw new InvalidProgramException($"Can't find {nameof(ModManager)}.{nameof(ModManager.LoadMods)}");
                if(linkerInit.orig is null) throw new InvalidProgramException($"Can't find {nameof(DataLinkerInitSystem)}.{nameof(DataLinkerInitSystem.Initialize)}");

                _ = harmony.Patch(loadMods.orig, prefix: new HarmonyMethod(loadMods.pre));
                _ = harmony.Patch(linkerInit.orig, postfix: new HarmonyMethod(linkerInit.post));

            } catch(Exception ex) {
                Logger.Orphan.HookError("Init Man", ex);
            }
        }

        public static void FullReset() {
            Logger.FullFlush(true, true);

            DataLinker.ClearLinkerCache();
            DataLinker.ClearProcessorCache();

            JsonModLoader.Reset();
            JsonModApplier.Reset();
            ReferenceFilesManager.Reset();

            Logger.SellOrphanage();

            IsInitFinished = false;
        }

        public static void OnInitFinished() {
            Logger.Orphan.Info("Init Finished!");
            if(PressXToJson.Config.LessLazyLoading) {
                //todo:
                // check what can cause Parts Presets to double load
                // confirm that we can track "seen" values, then do so
                // force load anybody we haven't yet seen
                // then unload all resources we're holding
                // -- Loader.FullReset is most of the way there, just run around and conifrm
                // Also confirm that the ApplyMods button calls the initer again.

                FullReset();
            } else {
                Logger.FullFlush(true);
                IsInitFinished = true;
            }
        }

    }
}
