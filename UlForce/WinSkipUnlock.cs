using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace VBRForceLock
{
    [BepInPlugin("vbr.force.winskip", "VBR Win Skip Everywhere", "1.0.0")]
    public class WinSkipPlugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo("VBR Win Skip Everywhere loaded (Win Skip aktif di semua difficulty)");
            var harmony = new Harmony("vbr.force.winskip");
            foreach (Type patch in new[] { typeof(MenuHandlerPatch), typeof(BattleUiSetMenuPatch), typeof(BattleUiButtonInteractivePatch) })
            {
                try
                {
                    harmony.PatchAll(patch);
                }
                catch (Exception e)
                {
                    Logger.LogError("[WinSkip] Gagal patch " + patch.Name + ": " + e);
                }
            }
        }
    }

    /// <summary>
    /// Di MenuHandler (menu SLG / map turn di mana tombol "Win Skip" berdampingan dengan "End Turn"),
    /// game secara default menonaktifkan _menus[8] jika difficulty > 1 (Normal, Hard, Very Hard, Nightmare).
    /// Patch ini memastikan _menus[8] selalu aktif dan masuk susunan navigasi tombol di semua difficulty.
    /// </summary>
    [HarmonyPatch(typeof(MenuHandler), "Start")]
    internal static class MenuHandlerPatch
    {
        private static bool Prefix(MenuHandler __instance)
        {
            try
            {
                __instance.isShow = false;

                Selectable[] menus = Traverse.Create(__instance).Field("_menus").GetValue<Selectable[]>();
                if (menus != null)
                {
                    // Pastikan tombol Win Skip (_menus[8]) selalu aktif
                    if (menus.Length > 8 && menus[8] != null)
                    {
                        menus[8].gameObject.SetActive(true);
                    }

                    var list = new List<Selectable>();
                    foreach (Selectable selectable in menus)
                    {
                        if (selectable != null && selectable.gameObject.activeSelf && selectable.interactable)
                        {
                            list.Add(selectable);
                        }
                    }

                    if (list.Count > 0)
                    {
                        NavigationSetters.HorizontalSetter(list.ToArray(), list[list.Count - 1], list[0]);
                    }
                }

                return false; // Lewati method asli agar _menus[8] tidak dimatikan
            }
            catch (Exception e)
            {
                WinSkipPlugin.Log?.LogError("[WinSkip] Error in MenuHandler.Start prefix: " + e);
                return true; // Fallback ke method asli jika terjadi exception
            }
        }
    }

    /// <summary>
    /// Di BattleUiHandler (menu battle / combat), tombol Win Skip (buttons[0]) dinonaktifkan jika difficulty > 1.
    /// SetMenu juga mengubah navigasi tombol untuk melewati buttons[0].
    /// Patch ini menjaga buttons[0] tetap aktif dan mempertahankan navigasi default tanpa melewatinya.
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem.BattleUiHandler), "SetMenu")]
    internal static class BattleUiSetMenuPatch
    {
        private static bool Prefix(BattleSystem.BattleUiHandler __instance)
        {
            try
            {
                List<GameObject> buttons = Traverse.Create(__instance).Field("buttons").GetValue<List<GameObject>>();
                if (buttons != null && buttons.Count > 0 && buttons[0] != null)
                {
                    buttons[0].SetActive(true);
                }

                __instance.ButtonInteractive(false);
                return false; // Lewati pembatalan navigasi & penonaktifan buttons[0]
            }
            catch (Exception e)
            {
                WinSkipPlugin.Log?.LogError("[WinSkip] Error in BattleUiHandler.SetMenu prefix: " + e);
                return true;
            }
        }
    }

    /// <summary>
    /// Di BattleUiHandler.ButtonInteractive(bool sw), game kembali memanggil buttons[0].SetActive(false)
    /// jika difficulty > 1. Postfix ini mengaktifkannya kembali.
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem.BattleUiHandler), "ButtonInteractive")]
    internal static class BattleUiButtonInteractivePatch
    {
        private static void Postfix(BattleSystem.BattleUiHandler __instance)
        {
            try
            {
                List<GameObject> buttons = Traverse.Create(__instance).Field("buttons").GetValue<List<GameObject>>();
                if (buttons != null && buttons.Count > 0 && buttons[0] != null)
                {
                    buttons[0].SetActive(true);
                }
            }
            catch (Exception e)
            {
                WinSkipPlugin.Log?.LogError("[WinSkip] Error in BattleUiHandler.ButtonInteractive postfix: " + e);
            }
        }
    }
}

