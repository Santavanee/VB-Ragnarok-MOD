using System;
using System.Collections.Generic;
using BepInEx;
using DataSet;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VBRForceLock
{
    // Mod untuk mempercantik list Equipment (EquipBlockHandler):
    // 1. Menambahkan icon item di sebelah nama (seperti panel info kiri).
    // 2. Memperbesar tinggi kotak baris (dari 45px menjadi 70px) agar lebih lega.
    // 3. Menambahkan daftar skill bawaan item (data.attach) pada baris baru di bawah status,
    //    lengkap dengan format resmi game (UnitInfoHandler.GetDrawSkill) dan auto-fit text.
    [BepInPlugin("vbr.force.equipicon", "VBR Equipment Icon & Skills", "1.1.1")]
    public class EquipmentIconPlugin : BaseUnityPlugin
    {
        private const string IconGoName = "UlForceEquipIcon";
        private const string SkillsGoName = "UlForceSkills";

        // Dimensi baru untuk baris kartu equipment
        public const float RowWidth = 400f;
        public const float RowHeight = 70f;       // Tinggi original: 45f
        public const float TopRowY = 47f;         // Posisi baris nama/icon (original: 22f, tinggi: 21f)
        public const float BottomRowY = 26f;      // Posisi baris status (original: 3f, tinggi: 19f)
        public const float SkillsRowY = 4f;       // Posisi baris skill baru (tinggi: 20f)
        public const float SkillsRowHeight = 20f;

        private const float TopRowHeight = 21f;
        private const float IconSize = 32f;
        private const float IconGap = 6f;

        // Warna teks skill: soft warm gold agar kontras di latar belakang gelap dan serasi dengan tema RPG
        private static readonly Color SkillTextColor = new Color(1.0f, 0.88f, 0.52f, 1f);

        private static BepInEx.Logging.ManualLogSource _log;
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        void Awake()
        {
            _log = Logger;
            Logger.LogInfo("VBR Equipment Icon & Skills loaded");
            var harmony = new Harmony("vbr.force.equipicon");

            try
            {
                harmony.PatchAll(typeof(ListExpanderPatch));
                Logger.LogInfo("[EquipIcon] ListExpanderPatch berhasil di-patch");
            }
            catch (Exception e)
            {
                Logger.LogWarning("[EquipIcon] Gagal patch ListExpanderPatch: " + e);
            }

            try
            {
                harmony.PatchAll(typeof(EquipRowPatch));
                Logger.LogInfo("[EquipIcon] EquipRowPatch berhasil di-patch");
            }
            catch (Exception e)
            {
                Logger.LogWarning("[EquipIcon] Gagal patch EquipRowPatch: " + e);
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

        // Patch saat coreListExpander diinisialisasi untuk EquiplistHandler:
        // Parameter target method adalah "o" (CoreSystem.ICoreListExpander).
        [HarmonyPatch(typeof(CoreSystem.coreListExpander), "SetInit")]
        private static class ListExpanderPatch
        {
            static void Prefix(CoreSystem.coreListExpander __instance, CoreSystem.ICoreListExpander o)
            {
                if (o is EquiplistHandler)
                {
                    var trav = Traverse.Create(__instance);
                    Vector2 preSize = trav.Field<Vector2>("preSize").Value;
                    trav.Field<Vector2>("preSize").Value = new Vector2(preSize.x, RowHeight);

                    GameObject prefab = trav.Field<GameObject>("_Prefab").Value;
                    if (prefab != null)
                    {
                        RectTransform rt = prefab.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.sizeDelta = new Vector2(rt.sizeDelta.x, RowHeight);
                        }
                        Transform p = prefab.transform.Find("Panel");
                        if (p != null)
                        {
                            RectTransform prt = p as RectTransform;
                            if (prt != null)
                            {
                                prt.sizeDelta = new Vector2(prt.sizeDelta.x, RowHeight);
                            }
                        }
                    }
                }
            }
        }

        // Patch saat baris equipment digambar ulang (DataDaraw)
        [HarmonyPatch(typeof(EquipBlockHandler), "DataDaraw")]
        private static class EquipRowPatch
        {
            static void Postfix(EquipBlockHandler __instance)
            {
                ItemDataSet data = __instance.data;
                if (data == null)
                {
                    return;
                }

                // 1. Pastikan expander menggunakan RowHeight jika telah aktif
                var reciever = __instance.GetComponent<CoreSystem.coreListExpanderReciever>();
                if (reciever != null)
                {
                    var expander = Traverse.Create(reciever).Field("_reciever").GetValue<CoreSystem.coreListExpander>();
                    if (expander != null)
                    {
                        var expTrav = Traverse.Create(expander);
                        Vector2 curPre = expTrav.Field<Vector2>("preSize").Value;
                        if (curPre.y != RowHeight)
                        {
                            expTrav.Field<Vector2>("preSize").Value = new Vector2(curPre.x, RowHeight);
                        }
                    }
                }

                // 2. Sesuaikan ukuran baris root
                RectTransform rootRect = __instance.transform as RectTransform;
                if (rootRect != null && rootRect.sizeDelta.y != RowHeight)
                {
                    rootRect.sizeDelta = new Vector2(rootRect.sizeDelta.x, RowHeight);
                }

                // 3. Sesuaikan Panel dan posisi baris anak
                Transform panel = __instance.transform.Find("Panel");
                if (panel == null)
                {
                    return;
                }

                RectTransform panelRect = panel as RectTransform;
                if (panelRect != null && panelRect.sizeDelta.y != RowHeight)
                {
                    panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, RowHeight);
                }

                Transform top = panel.Find("top");
                if (top != null)
                {
                    RectTransform topRect = top as RectTransform;
                    if (topRect != null && topRect.anchoredPosition.y != TopRowY)
                    {
                        topRect.anchoredPosition = new Vector2(topRect.anchoredPosition.x, TopRowY);
                    }
                }

                Transform bottom = panel.Find("bottom");
                if (bottom != null)
                {
                    RectTransform bottomRect = bottom as RectTransform;
                    if (bottomRect != null && bottomRect.anchoredPosition.y != BottomRowY)
                    {
                        bottomRect.anchoredPosition = new Vector2(bottomRect.anchoredPosition.x, BottomRowY);
                    }
                }

                // 4. Render Icon Item pada baris top
                CoreSystem.valueControl nameControl = Traverse.Create(__instance).Field("_name").GetValue<CoreSystem.valueControl>();
                if (nameControl != null && top != null)
                {
                    RectTransform nameRect = nameControl.transform as RectTransform;
                    if (nameRect != null)
                    {
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

                // 5. Render Daftar Skill pada baris skills
                UpdateSkillsRow(panel, data, nameControl);
            }

            private static void UpdateSkillsRow(Transform panel, ItemDataSet data, CoreSystem.valueControl nameControl)
            {
                Transform skillsTransform = panel.Find(SkillsGoName);
                Text skill1Text = null;
                Text skill2Text = null;

                if (skillsTransform == null)
                {
                    GameObject skillsGo = new GameObject(SkillsGoName, typeof(RectTransform));
                    RectTransform skillsRect = (RectTransform)skillsGo.transform;
                    skillsRect.SetParent(panel, worldPositionStays: false);
                    skillsRect.anchorMin = Vector2.zero;
                    skillsRect.anchorMax = Vector2.zero;
                    skillsRect.pivot = Vector2.zero;
                    skillsRect.anchoredPosition = new Vector2(0f, SkillsRowY);
                    skillsRect.sizeDelta = new Vector2(RowWidth, SkillsRowHeight);

                    Font font = null;
                    Material mat = null;
                    if (nameControl != null && nameControl.TextTarget != null)
                    {
                        font = nameControl.TextTarget.font;
                        mat = nameControl.TextTarget.material;
                    }

                    skill1Text = CreateSkillText(skillsGo.transform, "Skill1", font, mat);
                    skill2Text = CreateSkillText(skillsGo.transform, "Skill2", font, mat);
                    skillsTransform = skillsGo.transform;
                }
                else
                {
                    Transform t1 = skillsTransform.Find("Skill1");
                    if (t1 != null) skill1Text = t1.GetComponent<Text>();
                    Transform t2 = skillsTransform.Find("Skill2");
                    if (t2 != null) skill2Text = t2.GetComponent<Text>();
                }

                if (skill1Text == null || skill2Text == null)
                {
                    return;
                }

                string s1 = null;
                string s2 = null;

                if (data.attach != null)
                {
                    if (data.attach.Count > 0 && data.attach[0] != null && !string.IsNullOrEmpty(data.attach[0].id))
                    {
                        s1 = UnitInfoHandler.GetDrawSkill(data.attach[0], 4);
                        if (!string.IsNullOrEmpty(s1)) s1 = s1.Trim();
                    }
                    if (data.attach.Count > 1 && data.attach[1] != null && !string.IsNullOrEmpty(data.attach[1].id))
                    {
                        s2 = UnitInfoHandler.GetDrawSkill(data.attach[1], 4);
                        if (!string.IsNullOrEmpty(s2)) s2 = s2.Trim();
                    }
                }

                bool hasS1 = !string.IsNullOrEmpty(s1);
                bool hasS2 = !string.IsNullOrEmpty(s2);

                if (!hasS1 && !hasS2)
                {
                    skillsTransform.gameObject.SetActive(false);
                    return;
                }

                skillsTransform.gameObject.SetActive(true);

                if (hasS1 && !hasS2)
                {
                    // Hanya 1 skill: tampil penuh melebar
                    RectTransform r1 = skill1Text.rectTransform;
                    r1.anchoredPosition = new Vector2(16f, 0f);
                    r1.sizeDelta = new Vector2(370f, SkillsRowHeight);
                    skill1Text.text = s1;
                    skill1Text.gameObject.SetActive(true);

                    skill2Text.gameObject.SetActive(false);
                }
                else if (!hasS1 && hasS2)
                {
                    RectTransform r1 = skill1Text.rectTransform;
                    r1.anchoredPosition = new Vector2(16f, 0f);
                    r1.sizeDelta = new Vector2(370f, SkillsRowHeight);
                    skill1Text.text = s2;
                    skill1Text.gameObject.SetActive(true);

                    skill2Text.gameObject.SetActive(false);
                }
                else
                {
                    // 2 skill: 2 kolom berdampingan
                    RectTransform r1 = skill1Text.rectTransform;
                    r1.anchoredPosition = new Vector2(16f, 0f);
                    r1.sizeDelta = new Vector2(184f, SkillsRowHeight);
                    skill1Text.text = s1;
                    skill1Text.gameObject.SetActive(true);

                    RectTransform r2 = skill2Text.rectTransform;
                    r2.anchoredPosition = new Vector2(206f, 0f);
                    r2.sizeDelta = new Vector2(184f, SkillsRowHeight);
                    skill2Text.text = s2;
                    skill2Text.gameObject.SetActive(true);
                }
            }

            private static Text CreateSkillText(Transform parent, string name, Font font, Material material)
            {
                GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform rt = (RectTransform)go.transform;
                rt.SetParent(parent, worldPositionStays: false);
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = Vector2.zero;

                Text text = go.GetComponent<Text>();
                if (font != null) text.font = font;
                if (material != null) text.material = material;
                text.fontSize = 13;
                text.color = SkillTextColor;
                text.alignment = TextAnchor.MiddleLeft;
                text.raycastTarget = false;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 9;
                text.resizeTextMaxSize = 13;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;

                return text;
            }
        }
    }
}
