using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BattleSystem;
using DataSet;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VBRForceLock
{
    internal static class CustomUnitPortraits
    {
        private static readonly Dictionary<string, Sprite> ByUnitId = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite> ByAssetKey = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite> ByBattleName = new Dictionary<string, Sprite>();

        // Patch non-generic loaders or their UI consumers; never Load<T>/Resources.Load<T>.
        internal static readonly Type[] PatchTypes =
        {
            typeof(AtlasPortraitPatch),        // Atlas icons (image1[4]).
            typeof(UnitInfoPortraitPatch),     // Large unit detail portrait.
            typeof(RosterPortraitPatch),       // Army/roster icons.
            typeof(DivisionPortraitPatch),     // Division Info portrait.
            typeof(BattleHudPortraitPatch),    // Battle HUD.
            typeof(BattleViewerPortraitPatch)  // Battle viewer (image1[0]).
        };

        internal static void Load(BepInEx.Logging.ManualLogSource log)
        {
            LoadPortrait(log, LuluDefinition.Id, LuluDefinition.Name, LuluDefinition.PortraitKey, LuluDefinition.PortraitFileName);
            LoadPortrait(log, MaryDefinition.Id, MaryDefinition.Name, MaryDefinition.PortraitKey, MaryDefinition.PortraitFileName);
        }

        private static void LoadPortrait(BepInEx.Logging.ManualLogSource log, string id, string name, string key, string fileName)
        {
            try
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(dir, fileName);
                if (!File.Exists(path))
                {
                    log.LogWarning("[CustomUnit] File portrait tidak ketemu: " + path);
                    return;
                }
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(bytes))
                {
                    UnityEngine.Object.Destroy(tex);
                    log.LogWarning("[CustomUnit] Invalid portrait PNG: " + path);
                    return;
                }
                Sprite sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                ByUnitId[id] = sprite;
                ByAssetKey[key] = sprite;
                ByBattleName[name.Split(' ')[0]] = sprite;
            }
            catch (Exception e)
            {
                log.LogWarning("[CustomUnit] Gagal load portrait: " + e.Message);
            }
        }

        private static void ApplyUnitPortrait(object handler, UnitData unit, string imageField)
        {
            Sprite sprite;
            if (unit == null || !ByUnitId.TryGetValue(unit.id, out sprite)) return;
            ForceApplySprite(Traverse.Create(handler).Field(imageField).GetValue<Image>(), sprite);
        }

        [HarmonyPatch(typeof(BinaryLoad), nameof(BinaryLoad.LoadSpriteMultiple))]
        private static class AtlasPortraitPatch
        {
            private static bool Prefix(string fn, ref Sprite __result)
            {
                Sprite sprite;
                if (fn != null && ByAssetKey.TryGetValue(fn, out sprite))
                {
                    __result = sprite;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UnitInfoHandler), "DrawData", new Type[] { typeof(UnitData), typeof(UnitData[]) })]
        private static class UnitInfoPortraitPatch
        {
            private static void Postfix(UnitInfoHandler __instance, UnitData u)
            {
                ApplyUnitPortrait(__instance, u, "_unitImage");
            }
        }

        [HarmonyPatch(typeof(SingleUnitBlockHandler), "DataDaraw")]
        private static class RosterPortraitPatch
        {
            private static void Postfix(SingleUnitBlockHandler __instance)
            {
                ApplyUnitPortrait(__instance, __instance.data, "_Image");
            }
        }

        // Battle tags split names at spaces; match "Faceless" or "Swimsuit".
        [HarmonyPatch(typeof(StatusControl), "SetStatus")]
        private static class BattleHudPortraitPatch
        {
            private static void Postfix(StatusControl __instance)
            {
                MakeCode make = Traverse.Create(__instance).Field("make").GetValue<MakeCode>();
                Sprite sprite;
                if (make == null || make.name == null || __instance.image == null
                    || !ByBattleName.TryGetValue(make.name, out sprite)) return;
                ForceApplySprite(__instance.image.GetComponent<Image>(), sprite);
            }
        }

        [HarmonyPatch(typeof(UnitBlockHandler), "DataDaraw")]
        private static class DivisionPortraitPatch
        {
            private static void Postfix(UnitBlockHandler __instance)
            {
                ApplyUnitPortrait(__instance, __instance.data, "_Background");
            }
        }

        [HarmonyPatch(typeof(BinaryLoad), nameof(BinaryLoad.LoadSprite), new Type[] { typeof(string), typeof(float), typeof(float), typeof(BinaryLoad.LoadType), typeof(float) })]
        private static class BattleViewerPortraitPatch
        {
            private static bool Prefix(string path, ref Sprite __result)
            {
                if (path == null) return true;
                foreach (var entry in ByAssetKey)
                {
                    if (!path.EndsWith(entry.Key, StringComparison.Ordinal)) continue;
                    __result = entry.Value;
                    return false;
                }
                return true;
            }
        }

        // Only assign sprite, never overrideSprite (it leaks across reused hover UI).
        // Preserve the existing rect size because the PNG has different dimensions.
        private static void ForceApplySprite(Image img, Sprite sprite)
        {
            if (img == null || sprite == null) return;

            RectTransform rt = img.rectTransform;
            Vector2 sizeBefore = rt.sizeDelta;

            img.sprite = sprite;
            img.SetAllDirty();

            rt.sizeDelta = sizeBefore;
        }
    }
}
