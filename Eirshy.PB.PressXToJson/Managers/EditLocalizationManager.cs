using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;


using HarmonyLib;

using PhantomBrigade.Mods;
using PhantomBrigade.Data;

using Eirshy.PB.PressXToJson.Entities;

namespace Eirshy.PB.PressXToJson.Managers {
    internal static class EditLocalizationManager {
        static Type THIS => typeof(EditLocalizationManager);
        static Dictionary<string, List<LocalizationEntry>> _langLocs { get; set;  } = null;
        const string LIBRARY_LANG = "English";

        public static void Init(Harmony harmony) {
            _ApplyHooks(harmony);
        }
        private static void _ApplyHooks(Harmony harmony) {
            const BindingFlags FLAGS = BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
            var procLibLoc = (
                mm: typeof(ModManager).GetMethod(nameof(ModManager.ProcessLibraryEdits), FLAGS),
                post: THIS.GetMethod(nameof(ApplyLibraryLocalizations), FLAGS)
            );
            var procLoc = (
                mm: typeof(ModManager).GetMethod(nameof(ModManager.ProcessLocalizationEdits), FLAGS),
                post: THIS.GetMethod(nameof(ApplyLocalizations), FLAGS)
            );
            try {
                if(procLoc.mm is null) throw new InvalidProgramException($"Can't Find {nameof(procLibLoc)}!");
                if(procLoc.mm is null) throw new InvalidProgramException($"Can't Find {nameof(procLoc)}!");
                _ = harmony.Patch(procLoc.mm, postfix: new HarmonyMethod(procLoc.post));
                _ = harmony.Patch(procLibLoc.mm, postfix: new HarmonyMethod(procLibLoc.post));
            } catch(Exception ex) {
                Logger.Orphan.HookError("Edit Localization", ex);
            }
        }
        public static void Reset() {
            _langLocs.Clear();
        }

        public static void LoadLocalizations() {
            //This is a nightmare to build out into a LINQ, so just don't lol
            if(_langLocs is null) _langLocs = new Dictionary<string, List<LocalizationEntry>>();
            var insFiles = LoadingManager.AllJsonMods.Values.SelectMany(values=>values.Files);
            foreach(var insf in insFiles) {
                if(insf.Disabled || insf.Text is null) continue;
                foreach(var locfile in insf.Text) {
                    if(locfile.Value is null) continue;
                    foreach(var root in locfile.Value) {
                        if(root.Value is null) continue;
                        foreach(var lang in root.Value) {
                            if(lang.Value is null) continue;
                            var entry = lang.Value;
                            entry.Language = lang.Key;
                            entry.SectorKey = locfile.Key;
                            entry.EntryRoot = root.Key;
                            entry.Owner = insf;
                            var locs = _langLocs.GetOrNew(entry.Language.ToLower());
                            locs.Add(entry);
                        }
                    }
                }
            }

            Logger.Orphan.Info($"Registered {_langLocs.Count} languages with {_langLocs.Values.Sum(v=>v.Count)} entries");
        }
        internal static void ForceFinalizeLoadedMods() {
            if(_langLocs is null) {
                LoadingManager.FinalizeLoadedMods();
            }
        }

        public static void ApplyLibraryLocalizations(SortedDictionary<string, DataContainerTextSectorMain> sectors) {
            ForceFinalizeLoadedMods();
            if(!_langLocs.TryGetValue(LIBRARY_LANG?.ToLower(), out var locs)) return;//do we wanna default things?
            Logger.Orphan.Info($"Language {LIBRARY_LANG} has {locs.Count} entries!");
            foreach(var loc in locs) {
                if(!sectors.TryGetValue(loc.SectorKey, out var sec)) {
                    loc.Owner.AddError(new KeyNotFoundException($"Localization Sector Name '{loc.SectorKey}' Not Found"));
                    continue;
                }
                //---------------
                if(loc.Name != null) {
                    var named = sec.entries.GetOrNew(loc.EntryRoot + "__name");
                    named.text = loc.Name;
                }
                if(loc.Desc != null) {
                    var desc = sec.entries.GetOrNew(loc.EntryRoot + "__text");
                    desc.text = loc.Desc;
                }
            }
        }
        public static void ApplyLocalizations(string languageName, SortedDictionary<string, DataContainerTextSectorLocalization> sectors) {
            ForceFinalizeLoadedMods();
            if(!_langLocs.TryGetValue(languageName?.ToLower(), out var locs)) return;//do we wanna default things?
            foreach(var loc in locs) {
                if(!sectors.TryGetValue(loc.SectorKey, out var sec)) {
                    loc.Owner.AddError(new KeyNotFoundException($"Localization Sector Name '{loc.SectorKey}' Not Found"));
                    continue;
                }
                //---------------
                if(loc.Name != null) {
                    var named = sec.entries.GetOrNew(loc.EntryRoot + "__name");
                    named.text = loc.Name;
                }
                if(loc.Desc != null) {
                    var desc = sec.entries.GetOrNew(loc.EntryRoot + "__text");
                    desc.text = loc.Desc;
                }
            }
        }

    }
}
