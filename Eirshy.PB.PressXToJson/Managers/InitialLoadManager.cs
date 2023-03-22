using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using PhantomBrigade.Mods;
using PhantomBrigade.Game.Systems;

namespace Eirshy.PB.PressXToJson.Managers {
    internal static class InitialLoadManager {
        private static Type THIS => typeof(InitialLoadManager);
        public static bool IsInitFinished { get; private set; }

        public static void Init(Harmony harmony) {
            _ApplyHooks(harmony);
        }

        private static void _ApplyHooks(Harmony harmony) {
            const BindingFlags FLAGS = BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            var initer = typeof(DataLinkerInitSystem).GetMethod(nameof(DataLinkerInitSystem.Initialize), FLAGS);
            var postInit = THIS.GetMethod(nameof(OnInitFinished), FLAGS);

            try {
                if(initer is null) throw new InvalidProgramException($"Can't find {nameof(DataLinkerInitSystem)}.{nameof(DataLinkerInitSystem.Initialize)}");
                _ = harmony.Patch(initer, postfix: new HarmonyMethod(postInit));
            } catch(Exception ex) {
                Logger.Orphan.HookError("Init Man", ex);
            }
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

                JsonModLoader.LogAllErrors();
                Logger.FlushAndDeregAdopted();
                Logger.Orphan.Flush();
                JsonModLoader.FullReset();
            } else {
                IsInitFinished = true;
                FullFinishedFlush();
            }
        }

        public static void FullFinishedFlush() {
            if(!IsInitFinished) return;
            JsonModLoader.LogAllErrors();
            Logger.FlushAdopted();
            Logger.Orphan.Flush();
        }

        internal static void Reset() { }
    }
}
