﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using Newtonsoft.Json.Linq;

using PhantomBrigade.Mods;
using PhantomBrigade.Data;

using Eirshy.PB.PressXToJson.Entities;
using Eirshy.PB.PressXToJson.Config;
using Eirshy.PB.PressXToJson.DataProcessing;

namespace Eirshy.PB.PressXToJson.Managers {
    internal static class JsonModLoader {
        private static Type THIS => typeof(JsonModLoader);
        static readonly Dictionary<string, ModSettings> _allMods = new Dictionary<string, ModSettings>();
        public static IReadOnlyDictionary<string, ModSettings> AllJsonMods => _allMods;
        public static void LogAllErrors() => _allMods.Values.ForEach(mod => mod.LogAllErrors());

        public static void Init(Harmony harmony) {
            _ApplyHooks(harmony);
        }
        private static void _ApplyHooks(Harmony harmony) {
            const BindingFlags FLAGS = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
            var loadMods = (
                mm: typeof(ModManager).GetMethod(nameof(ModManager.LoadMods), FLAGS),
                pre: THIS.GetMethod(nameof(FullReset), FLAGS),
                post: THIS.GetMethod(nameof(FinalizeLoadedMods), FLAGS)
            );
            var tryloadEdits = (
                mm: typeof(ModManager).GetMethod(nameof(ModManager.TryLoadingConfigEdits), FLAGS),
                pre: THIS.GetMethod(nameof(LoadConfigEdits), FLAGS)
            );
            try {
                if(loadMods.mm is null) throw new InvalidProgramException($"Can't Find {nameof(loadMods)}!");
                if(tryloadEdits.mm is null) throw new InvalidProgramException($"Can't Find {nameof(tryloadEdits)}!");
                _ = harmony.Patch(loadMods.mm, prefix: new HarmonyMethod(loadMods.pre), postfix: new HarmonyMethod(loadMods.post));
                _ = harmony.Patch(tryloadEdits.mm, prefix: new HarmonyMethod(tryloadEdits.pre));
            } catch(Exception ex) {
                Logger.Orphan.HookError("Loader", ex);
            }
        }
        
        public static void FullReset() {
            LogAllErrors();
            _allMods.Clear();

            DataLinker.ClearLinkerCache();
            DataLinker.ClearProcessorCache();

            JsonModApplier.Reset();
            ReferenceFilesManager.Reset();
            InitialLoadManager.Reset();

            Logger.FlushAndDeregAdopted();
            Logger.SellOrphanage();
        }
        
        /// <remarks>
        /// Note: we are a valid hook *only* for any second or later calls of LoadMods, as
        /// our hooks are applied in the middle of a call of LoadMods.
        /// 
        /// Applier attempts to call us the first time it runs into stuff that implies we're
        /// loaded, assuming it hasn't gotten a shipment already.
        /// </remarks>
        public static void FinalizeLoadedMods() {
            JsonModApplier.LoadTypeInstructions();
            Logger.TransferOrphanage();
            PressXToJson.Config.SyncConfig();
        }


        public static void LoadConfigEdits(string id, string pathConfigEdits, ModLoadedData loadedData) {
            if(loadedData is null || !Directory.Exists(pathConfigEdits)) return;
            if(!loadedData.metadata.includesConfigEdits) return; //caller shouldn't've called us?

            var paths = new SortedSet<string>(
                Directory.EnumerateFiles(pathConfigEdits, "*.json", SearchOption.AllDirectories)
            );
            if(!paths.Any()) return; //no JSON files for us to read, ignore the mod.

            var preproc = new InstructionPreProcessor();
            var root = pathConfigEdits.Replace('\\', '/');
            root = root.Substring(0, root.Substring(0, root.Length-1).LastIndexOf('/')+1);

            var mod = new ModSettings(loadedData, root);
            mod.LoadSettingsFile();
            mod.LogSettings = PressXToJson.Config.Logs.Mods.TryGetValue(loadedData.metadata.id, out var le) ? le : LogsEntry.DEFAULT;
            _allMods.Add(loadedData.metadata.id, mod);

            mod.Files = paths
                .AsParallel()
                .Select((path, pi) => {
                    var shortpath = path.Replace('\\', '/').Replace(pathConfigEdits, "");
                    var shortDir = Path.GetDirectoryName(shortpath).Replace('\\', '/');

                    InstructionsFile file;
                    try {
                        var raw = File.ReadAllText(path);
                        file = raw.JsonTo<InstructionsFile>();
                    } catch(Exception ex) {
                        file = new InstructionsFile() {
                            PhysicalSource = path,
                            Owner = mod,
                            SourceRoute = shortDir,
                            SourceFile = Path.GetFileNameWithoutExtension(path),
                            Ins = Array.Empty<Instruction>(),
                        };
                        file.AddError(ex);
                        return file;
                    }

                    //File finishing
                    try {
                        file.FileLoadOrder = pi++;
                        file.Owner = mod;
                        file.PhysicalSource = path;
                        file.SourceRoute = shortDir;
                        file.SourceFile = Path.GetFileNameWithoutExtension(path);

                        file.SourceFileType = _routefile2type(file.SourceRoute, file.SourceFile);
                    } catch(Exception ex) {
                        file.AddError(ex);
                        return file;
                    }
                
                    //File shortcutted?
                    if(file.Disabled) {
                        file.Ins = Array.Empty<Instruction>();
                        return file;
                    }
                
                    //Instruction finishing
                    for(var insi = file.Ins.Count; insi-->0;) {
                        var ins = file.Ins[insi];
                        try {
                            ins.SourceIndex = insi;
                            ins.Owner = file;
                            if(ins.Disabled) continue;
                            if(ins.AsFile is null) ins.TargetType = file.SourceFileType;
                            else {
                                ins.TargetName = Path.GetFileNameWithoutExtension(ins.AsFile);
                                var fromRoot = ins.AsFile.StartsWith("~/");
                                var insDir = Path.GetDirectoryName(ins.AsFile).Replace('\\', '/');
                                ins.TargetRoute = fromRoot ? insDir.Substring(2) : Path.Combine(shortDir, insDir);
                                ins.TargetType = _routefile2type(ins.TargetRoute, ins.TargetName);
                                if(fromRoot) ins.Log.Info($"Root Route got type {ins.TargetType} : {ins.TargetRoute}");
                            }
                            preproc.PreProcess(ins);
                        } catch(Exception ex) {
                            ins.AddError(ex);
                        }
                    }
                    return file;
                })
                .ToList()
            ;
            //make sure our logs will be in the right order...
            mod.Files.Sort();

            //-----
            mod.Log.Info(mod.Stats);
        }



        private static Type _routefile2type(string route, string file) {
            var slashed = route + "/";
            var typePath =
                DataPathUtility.GetDataTypeFromPath($"{slashed}{file}")
                ?? DataPathUtility.GetDataTypeFromPath(slashed)
            ;
            if(typePath is null) return null;

            //TODO: support types from other libraries, ignoring if it's currently natively supported

            return typeof(ModManager).Assembly.GetType(
                $"{nameof(PhantomBrigade)}.{nameof(PhantomBrigade.Data)}.{typePath}"
                , throwOnError: false
            );
        }
    }
}
