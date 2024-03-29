﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

using HarmonyLib;

using PhantomBrigade.Mods;
using PhantomBrigade.Data;

using Eirshy.PB.PressXToJson.Entities;
using Eirshy.PB.PressXToJson.Exceptions;
using Eirshy.PB.PressXToJson.Config;
using Eirshy.PB.PressXToJson.Processing;
using Eirshy.PB.PressXToJson.Helpers;

namespace Eirshy.PB.PressXToJson.Managers {
    internal static class LoadingManager {
        private static Type THIS => typeof(LoadingManager);
        
        static Dictionary<string, ModData> _allMods { get; } = new Dictionary<string, ModData>();
        /// <summary>
        /// Keyed by Mod ID
        /// </summary>
        public static IReadOnlyDictionary<string, ModData> AllJsonMods => _allMods;
        public static void LogAllErrors() => _allMods.Values.ForEach(mod => mod.LogAllErrors());

        public static void Init(Harmony harmony) {
            _ApplyHooks(harmony);
        }
        private static void _ApplyHooks(Harmony harmony) {
            const BindingFlags FLAGS = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
            var loadMods = (
                mm: typeof(ModManager).GetMethod(nameof(ModManager.LoadMods), FLAGS),
                post: THIS.GetMethod(nameof(FinalizeLoadedMods), FLAGS)
            );
            var tryloadEdits = (
                mm: typeof(ModManager).GetMethod(nameof(ModManager.TryLoadingConfigEdits), FLAGS),
                pre: THIS.GetMethod(nameof(LoadConfigEdits), FLAGS)
            );
            try {
                if(loadMods.mm is null) throw new InvalidProgramException($"Can't Find {nameof(loadMods)}!");
                if(tryloadEdits.mm is null) throw new InvalidProgramException($"Can't Find {nameof(tryloadEdits)}!");
                _ = harmony.Patch(loadMods.mm, postfix: new HarmonyMethod(loadMods.post));
                _ = harmony.Patch(tryloadEdits.mm, prefix: new HarmonyMethod(tryloadEdits.pre));
            } catch(Exception ex) {
                Logger.Orphan.HookError("Loader", ex);
            }
        }
        
        public static void Reset() {
            _allMods.Clear();
        }
        
        /// <remarks>
        /// Note: we are a valid hook *only* for any second or later calls of LoadMods, as
        /// our hooks are applied in the middle of a call of LoadMods.
        /// 
        /// Applier attempts to call us the first time it runs into stuff that implies we're
        /// loaded, assuming it hasn't gotten a shipment already.
        /// </remarks>
        public static void FinalizeLoadedMods() {
            EditDataManager.LoadTypeInstructions();
            EditLocalizationManager.LoadLocalizations();
            Logger.TransferOrphanage();
            PressXToJson.Config.SyncConfig();
        }


        public static void LoadConfigEdits(string id, string pathConfigEdits, ModLoadedData loadedData) {
            if(loadedData is null || !Directory.Exists(pathConfigEdits)) return;
            if(!loadedData.metadata.includesConfigEdits) return; //caller shouldn't've called us?
            var pathPrefix = pathConfigEdits.Replace('\\', '/');
            var paths = new SortedSet<string>(
                Directory.EnumerateFiles(pathPrefix, "*.json", SearchOption.AllDirectories)
                .Select(s => s.Replace('\\', '/'))
            );
            if(!paths.Any()) return; //no JSON files for us to read, ignore the mod.
            
            var root = pathPrefix.Substring(0, pathPrefix.Substring(0, pathPrefix.Length-1).LastIndexOf('/')+1);
            var mod = new ModData(loadedData, root);
            mod.LoadSettingsFile();
            mod.LogSettings = PressXToJson.Config.Logs.Mods.TryGetValue(loadedData.metadata.id, out var le) ? le : LogsEntry.DEFAULT;
            _allMods.Add(loadedData.metadata.id, mod);

            var sw = Stopwatch.StartNew();
            mod.Files = paths
                .Select((path, pi) => {
                    var preproc = new InstructionPreProcessor();
                    var shortpath = path.Replace(pathPrefix, "");
                    var shortDir = Path.GetDirectoryName(shortpath);

                    InstructionsFile file;
                    try {
                        var raw = File.ReadAllText(path);
                        file = raw.JsonTo<InstructionsFile>();
                    } catch(Exception ex) {
                        file = new InstructionsFile() {
                            PhysicalSource = path,
                            Owner = mod,
                            SourceRoute = shortDir,
                            AmbientName = Path.GetFileNameWithoutExtension(path),
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
                        file.AmbientName = file.AsName ?? Path.GetFileNameWithoutExtension(path);
                        if(!string.IsNullOrWhiteSpace(file.AsType)) {
                            var at = AutoType.GetFrom(file.AsType, file.InstructionNamespace, file.InstructionAssembly);
                            file.AmbientType = at.ToType();
                        } else {
                            file.AmbientType = _routefile2type(file.SourceRoute, file.AmbientName);
                        }
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
                    for(var insi = file.Ins?.Count ?? 0; insi-->0;) {
                        var ins = file.Ins[insi];
                        try {
                            ins.SourceIndex = insi;
                            ins.Owner = file;
                            if(ins.Disabled) continue;
                            #region ins.TargetName = ...

                            if(ins.AsName != null) ins.TargetName = ins.AsName;
                            else if(ins.AsFile != null) {
                                ins.TargetName =  Path.GetFileNameWithoutExtension(ins.AsFile);
                            } else ins.TargetName = file.AmbientName;

                            #endregion
                            #region ins.TargetType = ...

                            if(!string.IsNullOrWhiteSpace(ins.AsType)) {
                                var at = AutoType.GetFrom(ins.AsType, file.InstructionNamespace, file.InstructionAssembly);
                                if(!at.IsValid) throw new AutoTypingException(ins.AsType);
                                ins.TargetType = at.ToType();

                            } else if(ins.AsFile != null) {
                                var fromRoot = ins.AsFile.StartsWith("~/");
                                var insDir = Path.GetDirectoryName(ins.AsFile).Replace('\\', '/');
                                var route = fromRoot ? insDir.Substring(2) : Path.Combine(shortDir, insDir);
                                ins.TargetType = _routefile2type(route, ins.TargetName);

                            } else ins.TargetType = file.AmbientType;

                            #endregion
                            preproc.PreProcess(ins);
                        } catch(Exception ex) {
                            ins.AddError(ex);
                        }
                    }
                    
                    //Localization Finishing is handled by the Localization Manager!
                    
                    return file;
                })
                .ToList()
            ;
            sw.Stop();
            //make sure our logs will be in the right order...
            mod.Files.Sort();

            //-----
            mod.Log.Info($"{mod.Stats} -- loaded in {sw.ElapsedMilliseconds} ms");
        }


        private static Type _routefile2type(string route, string file) {
            var slashed = route + "/";
            var typePath =
                DataPathUtility.GetDataTypeFromPath($"{slashed}{file}")
                ?? DataPathUtility.GetDataTypeFromPath(slashed)
            ;
            if(typePath is null) return null;

            return typeof(ModManager).Assembly.GetType(
                $"{nameof(PhantomBrigade)}.{nameof(PhantomBrigade.Data)}.{typePath}"
                , throwOnError: false
            );
        }
    }
}
