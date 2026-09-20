using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using DataSet;
using CoreSystem;

namespace VBRForceLock
{
    [BepInPlugin("vbr.force.heroicresearch", "VBR Heroic Spirit Research Unlock", "1.0.0")]
    public class HeroicSpiritResearchPlugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo("VBR Heroic Spirit Research Unlock loaded (Semua Heroic Spirit dapat direkrut manual via Research)");
            var harmony = new Harmony("vbr.force.heroicresearch");
            foreach (Type patch in new[]
            {
                typeof(ResearchControlStartPatch),
                typeof(ResearchControlSetModeRPatch),
                typeof(ResearchControlSetModeLPatch),
                typeof(ResearchControlMatrixOpenPatch),
                typeof(ResearchControlSetPagePatch),
                typeof(ResearchListHandlerSetPageOpenPatch),
                typeof(ResearchControlOnSelectMatrixPatch)
            })
            {
                try
                {
                    harmony.PatchAll(patch);
                }
                catch (Exception e)
                {
                    Logger.LogError("[HeroicResearch] Gagal patch " + patch.Name + ": " + e);
                }
            }
        }
    }

    internal static class HeroicSpiritResearchHelper
    {
        /// <summary>
        /// Membuka seluruh 25 node pada Page 8 (Heroic Spirits / 英霊) sehingga
        /// semua 51 unit Heroic Spirit dapat langsung dipilih dan direkrut manual.
        /// </summary>
        internal static void UnlockHeroicSpiritNodes(userDataSet userData)
        {
            if (userData == null || userData.ResearchMatrixData == null || userData.forceData == null)
                return;

            var labOpens = userData.forceData.labOpens;
            if (labOpens == null)
                return;

            while (labOpens.Count < 250)
            {
                labOpens.Add(false);
            }

            var labLevels = userData.forceData.labLevels;
            if (labLevels != null)
            {
                while (labLevels.Count < 250)
                {
                    labLevels.Add(0);
                }
            }

            foreach (var m in userData.ResearchMatrixData)
            {
                if (m != null && m.page == 8)
                {
                    if (m.index >= 0 && m.index < labOpens.Count)
                    {
                        labOpens[m.index] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Memastikan seluruh toggle tab (0 sampai 8) aktif dan dapat diklik.
        /// </summary>
        internal static void EnsureAllTogglesActive(ResearchListHandler listHandler)
        {
            if (listHandler == null)
                return;

            Toggle[] toggles = Traverse.Create(listHandler).Field("_Toggle").GetValue<Toggle[]>();
            if (toggles == null)
                return;

            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] != null)
                {
                    toggles[i].gameObject.SetActive(true);
                    toggles[i].interactable = true;
                }
            }

            var activeToggles = new List<Selectable>();
            foreach (var toggle in toggles)
            {
                if (toggle != null && toggle.gameObject.activeSelf && toggle.interactable)
                {
                    activeToggles.Add(toggle);
                }
            }
            if (activeToggles.Count > 0)
            {
                NavigationSetters.HorizontalSetter(activeToggles.ToArray(), activeToggles[activeToggles.Count - 1], activeToggles[0]);
            }
        }
    }

    /// <summary>
    /// Pada saat ResearchControl.Start, vanilla game mematikan _page7 jika Flags[142] == 1
    /// dan Toggle8 secara default bernilai active: false di scene level62.
    /// Patch ini memastikan _page7 dan Toggle8 (Heroic Spirits) selalu aktif.
    /// </summary>
    [HarmonyPatch(typeof(ResearchControl), "Start")]
    internal static class ResearchControlStartPatch
    {
        private static void Postfix(ResearchControl __instance)
        {
            try
            {
                GameObject page7 = Traverse.Create(__instance).Field("_page7").GetValue<GameObject>();
                if (page7 != null)
                {
                    page7.SetActive(true);
                }

                ResearchListHandler listHandler = Traverse.Create(__instance).Field("_researchListHandler").GetValue<ResearchListHandler>();
                HeroicSpiritResearchHelper.EnsureAllTogglesActive(listHandler);
            }
            catch (Exception ex)
            {
                HeroicSpiritResearchPlugin.Log?.LogError("[HeroicResearch] Error in ResearchControl.Start postfix: " + ex);
            }
        }
    }

    /// <summary>
    /// Memastikan semua tab (0-8) aktif dan interactable saat SetPageOpen dipanggil.
    /// </summary>
    [HarmonyPatch(typeof(ResearchListHandler), "SetPageOpen")]
    internal static class ResearchListHandlerSetPageOpenPatch
    {
        private static void Postfix(ResearchListHandler __instance)
        {
            try
            {
                HeroicSpiritResearchHelper.EnsureAllTogglesActive(__instance);
            }
            catch (Exception ex)
            {
                HeroicSpiritResearchPlugin.Log?.LogError("[HeroicResearch] Error in ResearchListHandler.SetPageOpen postfix: " + ex);
            }
        }
    }

    /// <summary>
    /// Tombol panah kanan (SetModeTypeR) di vanilla hanya berputar 0..6.
    /// Diubah agar berputar penuh 0..8 sehingga tab Heroic Spirit dapat dijangkau lewat tombol panah.
    /// </summary>
    [HarmonyPatch(typeof(ResearchControl), "SetModeTypeR")]
    internal static class ResearchControlSetModeRPatch
    {
        private static bool Prefix(ResearchControl __instance)
        {
            try
            {
                const int maxPage = 8;
                int page = Traverse.Create(typeof(ResearchControl)).Field("_page").GetValue<int>();
                page--;
                if (page < 0) page = maxPage;
                Traverse.Create(typeof(ResearchControl)).Field("_page").SetValue(page);

                AudioClip sound = Traverse.Create(__instance).Field("_soundSetClick").GetValue<AudioClip>();
                if (sound != null && SoundManager5.Instance != null)
                {
                    SoundManager5.Instance.PlaySystem(sound);
                }
                Traverse.Create(__instance).Method("SetPage", page).GetValue();
                return false;
            }
            catch (Exception ex)
            {
                HeroicSpiritResearchPlugin.Log?.LogError("[HeroicResearch] Error in SetModeTypeR prefix: " + ex);
                return true;
            }
        }
    }

    /// <summary>
    /// Tombol panah kiri (SetModeTypeL) di vanilla hanya berputar 0..6.
    /// Diubah agar berputar penuh 0..8 sehingga tab Heroic Spirit dapat dijangkau lewat tombol panah.
    /// </summary>
    [HarmonyPatch(typeof(ResearchControl), "SetModeTypeL")]
    internal static class ResearchControlSetModeLPatch
    {
        private static bool Prefix(ResearchControl __instance)
        {
            try
            {
                const int maxPage = 8;
                int page = Traverse.Create(typeof(ResearchControl)).Field("_page").GetValue<int>();
                page++;
                if (page > maxPage) page = 0;
                Traverse.Create(typeof(ResearchControl)).Field("_page").SetValue(page);

                AudioClip sound = Traverse.Create(__instance).Field("_soundSetClick").GetValue<AudioClip>();
                if (sound != null && SoundManager5.Instance != null)
                {
                    SoundManager5.Instance.PlaySystem(sound);
                }
                Traverse.Create(__instance).Method("SetPage", page).GetValue();
                return false;
            }
            catch (Exception ex)
            {
                HeroicSpiritResearchPlugin.Log?.LogError("[HeroicResearch] Error in SetModeTypeL prefix: " + ex);
                return true;
            }
        }
    }

    /// <summary>
    /// Membuka node-node Page 8 ketika MatrixOpen dipanggil.
    /// </summary>
    [HarmonyPatch(typeof(ResearchControl), "MatrixOpen")]
    internal static class ResearchControlMatrixOpenPatch
    {
        private static void Postfix(ResearchControl __instance)
        {
            try
            {
                var userData = GameDatas.Instance != null ? GameDatas.Instance.userData : null;
                if (userData != null)
                {
                    HeroicSpiritResearchHelper.UnlockHeroicSpiritNodes(userData);
                }
            }
            catch (Exception ex)
            {
                HeroicSpiritResearchPlugin.Log?.LogError("[HeroicResearch] Error in MatrixOpen postfix: " + ex);
            }
        }
    }

    /// <summary>
    /// Memastikan semua node Page 8 terbuka saat tab Page 8 dibuka.
    /// </summary>
    [HarmonyPatch(typeof(ResearchControl), "SetPage")]
    internal static class ResearchControlSetPagePatch
    {
        private static void Postfix(ResearchControl __instance, int n)
        {
            try
            {
                if (n == 8)
                {
                    var userData = GameDatas.Instance != null ? GameDatas.Instance.userData : null;
                    if (userData != null)
                    {
                        HeroicSpiritResearchHelper.UnlockHeroicSpiritNodes(userData);
                    }
                }
            }
            catch (Exception ex)
            {
                HeroicSpiritResearchPlugin.Log?.LogError("[HeroicResearch] Error in SetPage postfix: " + ex);
            }
        }
    }

    /// <summary>
    /// Pada saat memilih node di matrix, jika node sudah selesai direkrut (level >= levelmax),
    /// tetap tampilkan info unit terakhir yang direkrut alih-alih layar info kosong.
    /// </summary>
    [HarmonyPatch(typeof(ResearchControl), "OnSelectMatrix")]
    internal static class ResearchControlOnSelectMatrixPatch
    {
        private static bool Prefix(ResearchControl __instance, coreButtonEventData ev)
        {
            try
            {
                if (ev.buttonIndex == -1) return false;

                ResearchListHandler listHandler = Traverse.Create(__instance).Field("_researchListHandler").GetValue<ResearchListHandler>();
                if (listHandler == null) return true;

                ResearchMatrixSet data = listHandler.GetData(ev.buttonIndex);
                if (data == null || !data.open) return false;

                UnitData unit = data.GetUnit(data.level);
                if (unit == null && data.level > 0)
                {
                    unit = data.GetUnit(data.level - 1);
                }

                if (unit != null)
                {
                    UnitInfoHandler infoHandler = Traverse.Create(__instance).Field("_InfoUnitPrefab").GetValue<UnitInfoHandler>();
                    if (infoHandler != null)
                    {
                        infoHandler.DrawData(unit, null);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                HeroicSpiritResearchPlugin.Log?.LogError("[HeroicResearch] Error in OnSelectMatrix prefix: " + ex);
                return true;
            }
        }
    }
}

