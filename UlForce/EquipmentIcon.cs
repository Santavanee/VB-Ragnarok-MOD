using System;
using System.Collections.Generic;
using BepInEx;
using DataSet;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VBRForceLock
{
    // Nambahin icon item (sama kayak yang ditampilin gede di panel info kiri,
    // "panel/icon" pada EquiplistInfoHandler, 80x80, dari data.image[0]) ke
    // SETIAP baris di list Equipment (EquipBlockHandler) — prefab baris aslinya
    // gak punya slot Image buat ini sama sekali (cuma ada icon kategori kecil
    // 20x20 di field "rare"), jadi GameObject Image-nya dibikin manual di
    // runtime & ditaruh di sebelah kiri teks nama.
    //
    // Struktur baris (hasil dump diagnostik "Panel/top/name"):
    //   Panel(400x45) > top(400x21, anchor bottom-left) > name(anchoredPos 3,-2 ; size 225x25 ; anchor/pivot 0,0)
    // Icon ditaruh persis di posisi "name" yang lama (anchoredPos.x=3), lalu
    // "name" digeser ke kanan sejauh lebar icon + gap biar gak numpuk.
    [BepInPlugin("vbr.force.equipicon", "VBR Equipment Icon", "1.0.0")]
    public class EquipmentIconPlugin : BaseUnityPlugin
    {
        private const string IconGoName = "UlForceEquipIcon";
        // Tinggi baseline baris "top" (hasil dump diagnostik: Panel/top sizeDelta.y=21).
        // Dipakai buat nge-center icon walau IconSize lebih gede dari 21 (bakal
        // dikit overflow ke atas/bawah baris, sama kayak teks nama yang emang
        // udah overflow 25 > 21 tanpa masalah visual).
        private const float TopRowHeight = 21f;
        private const float IconSize = 32f;
        private const float IconGap = 6f;

        private static BepInEx.Logging.ManualLogSource _log;
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        void Awake()
        {
            _log = Logger;
            Logger.LogInfo("VBR Equipment Icon loaded");
            try
            {
                new Harmony("vbr.force.equipicon").PatchAll(typeof(EquipRowIconPatch));
            }
            catch (Exception e)
            {
                Logger.LogWarning("[EquipIcon] Gagal patch EquipRowIconPatch: " + e);
            }
        }

        private static Sprite GetCachedSprite(string key)
        {
            Sprite sprite;
            if (_spriteCache.TryGetValue(key, out sprite))
            {
                return sprite;
            }
            sprite = BinaryLoad.Load<Sprite>(key);
            _spriteCache[key] = sprite;
            return sprite;
        }

        [HarmonyPatch(typeof(EquipBlockHandler), "DataDaraw")]
        private static class EquipRowIconPatch
        {
            static void Postfix(EquipBlockHandler __instance)
            {
                ItemDataSet data = __instance.data;
                if (data == null)
                {
                    return;
                }

                CoreSystem.valueControl nameControl = Traverse.Create(__instance).Field("_name").GetValue<CoreSystem.valueControl>();
                if (nameControl == null)
                {
                    return;
                }

                RectTransform nameRect = nameControl.transform as RectTransform;
                Transform top = nameControl.transform.parent;
                if (nameRect == null || top == null)
                {
                    return;
                }

                Transform existing = top.Find(IconGoName);
                Image iconImage;
                if (existing == null)
                {
                    GameObject go = new GameObject(IconGoName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    RectTransform iconRect = (RectTransform)go.transform;
                    iconRect.SetParent(top, worldPositionStays: false);
                    iconRect.anchorMin = new Vector2(0f, 0f);
                    iconRect.anchorMax = new Vector2(0f, 0f);
                    iconRect.pivot = new Vector2(0f, 0f);
                    iconRect.sizeDelta = new Vector2(IconSize, IconSize);
                    iconRect.anchoredPosition = new Vector2(nameRect.anchoredPosition.x, (TopRowHeight - IconSize) / 2f);
                    iconRect.SetSiblingIndex(nameRect.GetSiblingIndex());

                    iconImage = go.GetComponent<Image>();
                    iconImage.preserveAspect = true;

                    nameRect.anchoredPosition = new Vector2(nameRect.anchoredPosition.x + IconSize + IconGap, nameRect.anchoredPosition.y);
                    nameRect.sizeDelta = new Vector2(nameRect.sizeDelta.x - (IconSize + IconGap), nameRect.sizeDelta.y);
                }
                else
                {
                    iconImage = existing.GetComponent<Image>();
                }

                if (data.image != null && data.image.Count > 0 && !string.IsNullOrEmpty(data.image[0]))
                {
                    Sprite sprite = GetCachedSprite(data.image[0]);
                    iconImage.sprite = sprite;
                    iconImage.enabled = sprite != null;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }
        }
    }
}
