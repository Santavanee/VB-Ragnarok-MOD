using System;
using System.Collections;
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
        private static readonly Dictionary<string, Texture2D> TextureByAssetKey = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Sprite> SpriteVariants = new Dictionary<string, Sprite>();

        // Patch non-generic loaders or their UI consumers; never Load<T>/Resources.Load<T>.
        internal static readonly Type[] PatchTypes =
        {
            typeof(AtlasPortraitPatch),        // Atlas icons (image1[4]).
            typeof(UnitInfoPortraitPatch),     // Large unit detail portrait.
            typeof(UnitInfoMemberIconsPatch),  // Six member icons in unit detail.
            typeof(RosterPortraitPatch),       // Army/roster icons.
            typeof(DivisionPortraitPatch),     // Division Info portrait.
            typeof(BattleHudPortraitPatch),    // Battle HUD.
            typeof(BattleViewerPortraitPatch), // Battle sprites/cut-ins (image1[0]).
            typeof(TacticsPortraitPatch),      // Tactics hover icon.
            typeof(SkillCheckPortraitPatch),   // Pre-battle skill summary.
            typeof(BattleResultPortraitPatch)  // Post-battle unit list.
        };

        internal static void Load(BepInEx.Logging.ManualLogSource log)
        {
            LoadPortrait(log, LuluDefinition.Id, LuluDefinition.Name, LuluDefinition.PortraitKey, LuluDefinition.PortraitFileName);
            LoadPortrait(log, MaryDefinition.Id, MaryDefinition.Name, MaryDefinition.PortraitKey, MaryDefinition.PortraitFileName);
            foreach (bool dark in new[] { false, true })
                LoadPortrait(log, NannaDefinition.Id(dark), NannaDefinition.Name(dark), NannaDefinition.PortraitKey(dark), NannaDefinition.PortraitFileName(dark));
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
                TextureByAssetKey[key] = tex;
                ByBattleName[name.Split(' ')[0]] = sprite;
            }
            catch (Exception e)
            {
                log.LogWarning("[CustomUnit] Gagal load portrait: " + e.Message);
            }
        }

        private static Sprite GetSpriteVariant(string key, float pivotX, float pivotY, float pixelsPerUnit)
        {
            Texture2D texture;
            if (!TextureByAssetKey.TryGetValue(key, out texture)) return null;

            // Battle sprites use native pixels / PPU as world size. Nanna's high-resolution
            // portraits must occupy the same 195-pixel maximum extent as Mary's sprite.
            // Keep the original texture resolution for the UI and Division face crops.
            if (key == NannaDefinition.PortraitKey(false) || key == NannaDefinition.PortraitKey(true))
                pixelsPerUnit *= Mathf.Max(texture.width, texture.height) / 195f;

            string variantKey = key + "|" + pivotX + "|" + pivotY + "|" + pixelsPerUnit;
            Sprite sprite;
            if (!SpriteVariants.TryGetValue(variantKey, out sprite))
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(pivotX, pivotY),
                    pixelsPerUnit);
                SpriteVariants[variantKey] = sprite;
            }
            return sprite;
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

        private static void ApplyIconPortrait(Image image, string assetKey)
        {
            Sprite sprite;
            if (image != null && assetKey != null && ByAssetKey.TryGetValue(assetKey, out sprite))
                ForceApplySprite(image, sprite);
        }

        [HarmonyPatch(typeof(UnitInfoHandler), "DrawData", new Type[] { typeof(UnitData), typeof(UnitData[]) })]
        private static class UnitInfoPortraitPatch
        {
            private static void Postfix(UnitInfoHandler __instance, UnitData u)
            {
                ApplyUnitPortrait(__instance, u, "_unitImage");
            }
        }

        [HarmonyPatch(typeof(UnitInfoHandler), "SetIcons")]
        private static class UnitInfoMemberIconsPatch
        {
            private static void Postfix(UnitInfoHandler __instance, UnitData[] u)
            {
                GameObject[] icons = Traverse.Create(__instance).Field("_Icons").GetValue<GameObject[]>();
                if (icons == null || u == null) return;
                int count = Math.Min(icons.Length, u.Length);
                for (int i = 0; i < count; i++)
                {
                    if (u[i] != null)
                        ApplyIconPortrait(icons[i].GetComponent<Image>(), u[i].image1[4]);
                }
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
                // Vanilla Celestial Nanna has the same name but a different native resource.
                if ((make.name == "Celestial" || make.name == "Eclipse")
                    && !NannaDefinition.MatchesBattleImage(make.name, make.image)) return;
                ForceApplySprite(__instance.image.GetComponent<Image>(), sprite);
            }
        }

        [HarmonyPatch(typeof(UnitBlockHandler), "DataDaraw")]
        private static class DivisionPortraitPatch
        {
            private static void Postfix(UnitBlockHandler __instance)
            {
                UnitData unit = __instance.data;
                Sprite source;
                if (unit == null || !ByUnitId.TryGetValue(unit.id, out source)) return;
                Image target = Traverse.Create(__instance).Field("_Background").GetValue<Image>();
                if (target == null) return;
                ForceApplySprite(target, GetDivisionPortrait(unit.id, source, target.rectTransform.rect));
            }
        }

        private static Sprite GetDivisionPortrait(string id, Sprite source, Rect target)
        {
            if (target.width <= 0f || target.height <= 0f) return source;

            // Face anchors use texture coordinates (Y starts at the bottom).
            // Match the wide native Division portrait with a crop, not a squashed full sprite.
            float centerX = 0.5f;
            float centerY = 0.46f;
            float widthFraction = 0.82f;
            if (id == LuluDefinition.Id)
            {
                centerY = 0.43f;
                widthFraction = 1f;
            }
            else if (id == MaryDefinition.Id)
            {
                centerY = 0.62f;
                widthFraction = 0.9f;
            }

            Texture2D texture = source.texture;
            float aspect = target.width / target.height;
            float width = texture.width * widthFraction;
            float height = width / aspect;
            if (height > texture.height)
            {
                height = texture.height;
                width = height * aspect;
            }
            Rect crop = new Rect(
                Mathf.Clamp(texture.width * centerX - width * 0.5f, 0f, texture.width - width),
                Mathf.Clamp(texture.height * centerY - height * 0.5f, 0f, texture.height - height),
                width, height);
            string key = "division|" + id + "|" + aspect;
            Sprite sprite;
            if (!SpriteVariants.TryGetValue(key, out sprite))
            {
                sprite = Sprite.Create(texture, crop, new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
                SpriteVariants[key] = sprite;
            }
            return sprite;
        }

        [HarmonyPatch(typeof(BinaryLoad), nameof(BinaryLoad.LoadSprite), new Type[] { typeof(string), typeof(float), typeof(float), typeof(BinaryLoad.LoadType), typeof(float) })]
        private static class BattleViewerPortraitPatch
        {
            private static bool Prefix(string path, float pw, float ph, float pp, ref Sprite __result)
            {
                if (path == null) return true;
                foreach (var entry in ByAssetKey)
                {
                    if (!path.EndsWith(entry.Key, StringComparison.Ordinal)) continue;
                    __result = GetSpriteVariant(entry.Key, pw, ph, pp);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TacticsControl), "OnEnter")]
        private static class TacticsPortraitPatch
        {
            private static void Postfix(TacticsControl __instance, int i)
            {
                DivisionData division = Traverse.Create(__instance).Field("dv").GetValue<DivisionData>();
                GameObject icon = Traverse.Create(__instance).Field("icon").GetValue<GameObject>();
                int unitIndex = i / 10;
                if (division == null || icon == null || unitIndex < 0 || unitIndex >= division.unit.Length) return;
                UnitData unit = division.unit[unitIndex];
                if (unit != null) ApplyIconPortrait(icon.GetComponent<Image>(), unit.image1[4]);
            }
        }

        [HarmonyPatch(typeof(SkillCheckControl), "Init")]
        private static class SkillCheckPortraitPatch
        {
            private static void Postfix(SkillCheckControl __instance, List<DiviSionInfoCode> _div)
            {
                if (_div == null) return;
                foreach (DiviSionInfoCode division in _div)
                {
                    List<GameObject> targets = division.powerd != 0 ? __instance._div1 : __instance._div0;
                    int count = Math.Min(targets.Count, division.icon.Count);
                    for (int i = 0; i < count; i++)
                        ApplyIconPortrait(targets[i].GetComponent<Image>(), division.icon[i]);
                }
            }
        }

        [HarmonyPatch(typeof(UnitListControl), "ShowList")]
        private static class BattleResultPortraitPatch
        {
            private static void Postfix(UnitListControl __instance, ResultDivisionListCode div, ref IEnumerator __result)
            {
                __result = new RefreshingEnumerator(__result, __instance, div);
            }

            private sealed class RefreshingEnumerator : IEnumerator
            {
                private readonly IEnumerator _original;
                private readonly UnitListControl _handler;
                private readonly ResultDivisionListCode _division;

                internal RefreshingEnumerator(IEnumerator original, UnitListControl handler, ResultDivisionListCode division)
                {
                    _original = original;
                    _handler = handler;
                    _division = division;
                }

                public object Current { get { return _original.Current; } }

                public bool MoveNext()
                {
                    bool hasNext = _original.MoveNext();
                    ApplyResultIcons(_handler.enemysUnitIcon, _division.enemysIcon);
                    ApplyResultIcons(_handler.playerUnitIcon, _division.playerIcon);
                    return hasNext;
                }

                public void Reset()
                {
                    _original.Reset();
                }
            }

            private static void ApplyResultIcons(List<GameObject> targets, List<string> assetKeys)
            {
                int count = Math.Min(targets.Count, assetKeys.Count);
                for (int i = 0; i < count; i++)
                    ApplyIconPortrait(targets[i].GetComponent<Image>(), assetKeys[i].TrimEnd('*'));
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
