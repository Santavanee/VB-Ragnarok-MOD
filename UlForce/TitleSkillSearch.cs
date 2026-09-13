using System;
using System.Collections.Generic;
using BepInEx;
using CoreSystem;
using DataSet;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VBRForceLock
{
    [BepInPlugin("vbr.force.titlesearch", "VBR Title Skill Search", "1.0.0")]
    public class TitleSkillSearchPlugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            var harmony = new Harmony("vbr.force.titlesearch");
            foreach (Type patch in new[] { typeof(SearchOpenPatch), typeof(SearchFilterPatch) })
            {
                try { harmony.PatchAll(patch); }
                catch (Exception e) { Logger.LogError("[TitleSearch] " + e); }
            }
        }

        [HarmonyPatch(typeof(MedallionControl), "OnOpen")]
        private static class SearchOpenPatch
        {
            private static void Postfix(MedallionControl __instance)
            {
                try
                {
                    var search = __instance.GetComponent<TitleSkillSearch>();
                    if (search == null)
                    {
                        search = __instance.gameObject.AddComponent<TitleSkillSearch>();
                        search.Initialize(__instance);
                    }
                    search.ResetSearch();
                }
                catch (Exception e) { Log.LogError("[TitleSearch] UI: " + e); }
            }
        }

        [HarmonyPatch(typeof(TitleListBlockHandler), "DrawData")]
        private static class SearchFilterPatch
        {
            private static void Prefix(TitleListBlockHandler __instance, ref List<TitleDataSet> d)
            {
                var search = __instance.GetComponentInParent<TitleSkillSearch>();
                if (search == null || search.Query.Length == 0) return;
                var filtered = new List<TitleDataSet>();
                foreach (var title in d)
                    if (search.Matches(title)) filtered.Add(title);
                d = filtered;
            }
        }
    }

    // Instance state belongs to the modal, so changing saves/scenes cannot reuse old data.
    public class TitleSkillSearch : MonoBehaviour
    {
        private InputField input;
        private Text summary;
        private TitleListHandler titles;
        private MedallionBlockHandler[] medals;
        private CanvasGroup[] highlights;
        private readonly HashSet<int> matchingMedals = new HashSet<int>();
        public string Query { get; private set; } = string.Empty;

        public void Initialize(MedallionControl control)
        {
            titles = Traverse.Create(control).Field("_titleListHandler").GetValue<TitleListHandler>();
            var list = Traverse.Create(control).Field("_MedallionListHandler").GetValue<MedallionListHandler>();
            medals = Traverse.Create(list).Field("_medals").GetValue<MedallionBlockHandler[]>();
            highlights = new CanvasGroup[medals.Length];
            // Put our alpha on a separate component; never overwrite game-owned UI state.
            for (int i = 0; i < medals.Length; i++)
                if (medals[i] != null) highlights[i] = medals[i].gameObject.AddComponent<CanvasGroup>();

            var source = Traverse.Create(control).Field("_InputFieldName").GetValue<InputField>();
            Font font = source.textComponent.font;
            var panel = MakeRect("UlForceTitleSearch", list.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-16, 25), new Vector2(285, 90));
            panel.gameObject.AddComponent<Image>().color = new Color(0.035f, 0.055f, 0.075f, 0.96f);
            MakeText("Label", panel, font, "Search title skill", new Vector2(10, -5), new Vector2(235, 22));
            var field = MakeRect("Search", panel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -30), new Vector2(229, 28));
            var background = field.gameObject.AddComponent<Image>();
            background.color = new Color(0.95f, 0.94f, 0.88f);
            input = field.gameObject.AddComponent<InputField>();
            input.targetGraphic = background;
            var text = MakeText("Text", field, font, "", new Vector2(6, -2), new Vector2(217, 24));
            text.color = Color.black;
            text.supportRichText = false;
            input.textComponent = text;
            var placeholder = MakeText("Placeholder", field, font, "e.g. Dimension Slash", new Vector2(6, -2), new Vector2(217, 24));
            placeholder.color = new Color(0.35f, 0.35f, 0.35f);
            input.placeholder = placeholder;
            input.characterLimit = 80;
            input.lineType = InputField.LineType.SingleLine;
            input.onValueChanged.AddListener(Refresh);

            var clearRect = MakeRect("Clear", panel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(245, -30), new Vector2(30, 28));
            var clearImage = clearRect.gameObject.AddComponent<Image>();
            clearImage.color = new Color(0.15f, 0.36f, 0.48f);
            var clear = clearRect.gameObject.AddComponent<Button>();
            clear.targetGraphic = clearImage;
            MakeText("Text", clearRect, font, "X", new Vector2(8, -2), new Vector2(22, 24));
            clear.onClick.AddListener(delegate { input.text = string.Empty; });
            summary = MakeText("Summary", panel, font, "", new Vector2(10, -63), new Vector2(265, 22));
            summary.fontSize = 12;
            TitleSkillSearchPlugin.Log.LogInfo("[TitleSearch] Search bar created");
        }

        public void ResetSearch()
        {
            if (input == null) return;
            input.text = string.Empty;
            Refresh(string.Empty);
        }

        public bool Matches(TitleDataSet title)
        {
            if (Query.Length == 0) return true;
            if (title == null || string.IsNullOrEmpty(title.attach.id) || title.attach.id == HDDataSetDef.NULL) return false;
            var passive = ExValue.GetPassiveDataSet(title.attach.id);
            return passive != null && !string.IsNullOrEmpty(passive.name)
                && passive.name.IndexOf(Query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Refresh(string value)
        {
            Query = value.Trim();
            matchingMedals.Clear();
            int count = 0;
            var user = GameDatas.Instance.userData;
            if (Query.Length != 0)
            {
                foreach (var title in user.titelData)
                {
                    if (!Matches(title) || title.medal.Count == 0) continue;
                    int medal = title.medal[0];
                    if (medal <= 0 || !user.forceData.IsMedals(medal)) continue;
                    matchingMedals.Add(medal);
                    count++;
                }
            }
            for (int i = 0; i < highlights.Length; i++)
                if (highlights[i] != null)
                    highlights[i].alpha = Query.Length == 0 || matchingMedals.Contains(i + 1) ? 1f : 0.2f;
            summary.text = Query.Length == 0 ? "Type a skill, then choose a medallion."
                : count == 0 ? "No matching titles in unlocked medallions."
                : count + " titles / " + matchingMedals.Count + " medallions";
            titles.OnCallUpdareDraw();
        }

        private static RectTransform MakeRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private static Text MakeText(string name, Transform parent, Font font, string value, Vector2 position, Vector2 size)
        {
            var rect = MakeRect(name, parent, new Vector2(0, 1), new Vector2(0, 1), position, size);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }
    }
}
