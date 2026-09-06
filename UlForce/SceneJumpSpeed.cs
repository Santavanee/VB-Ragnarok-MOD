using System;
using AvgSystem;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VBRForceLock
{
    // "Scene jumping" (skip otomatis lewat cerita yang udah pernah dibaca,
    // misal abis load save di tengah chapter) itu di-throttle PERSIS 1 baris
    // cerita (cut) per frame render lewat `yield return 0` di coroutine
    // internal avg-nya (AvgDataModel.execSceneView) — independen dari
    // kecepatan CPU. Makin cepet FPS-nya jalan, makin cepet jump-nya kelar.
    // Solusi: lepas cap FPS + matiin VSync SELAMA panel scene-jump tampil,
    // balikin lagi persis pas kelar (atau dibatalin manual).
    [BepInPlugin("vbr.force.scenejumpspeed", "VBR Scene Jump Speed", "1.0.0")]
    public class SceneJumpSpeedPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Logger.LogInfo("VBR Scene Jump Speed loaded");
            Harmony harmony = new Harmony("vbr.force.scenejumpspeed");
            try
            {
                harmony.PatchAll(typeof(ShowSceneJumpPatch));
                harmony.PatchAll(typeof(OnSceneJumpStopPatch));
            }
            catch (Exception e)
            {
                Logger.LogWarning("[SceneJumpSpeed] Gagal patch: " + e);
            }
        }
    }

    static class SceneJumpSpeedState
    {
        private const int BoostedFps = 1000;

        private static bool _overriding;
        private static int _savedTargetFrameRate;
        private static int _savedVSyncCount;

        internal static void Boost()
        {
            if (_overriding)
                return;
            _overriding = true;
            _savedTargetFrameRate = Application.targetFrameRate;
            _savedVSyncCount = QualitySettings.vSyncCount;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = BoostedFps;
        }

        internal static void Restore()
        {
            if (!_overriding)
                return;
            _overriding = false;
            QualitySettings.vSyncCount = _savedVSyncCount;
            Application.targetFrameRate = _savedTargetFrameRate;
        }
    }

    // Dipanggil tiap cut: b=true pas mulai skip (tampilin panel), b=false
    // begitu udah nyampe konten yang belum dibaca (panel disembunyiin lagi).
    [HarmonyPatch(typeof(AvgUiControl), "ShowSceneJump")]
    class ShowSceneJumpPatch
    {
        static void Postfix(bool b)
        {
            if (b)
                SceneJumpSpeedState.Boost();
            else
                SceneJumpSpeedState.Restore();
        }
    }

    // Dipanggil kalau player nge-cancel scene jump manual (tombol di panel).
    [HarmonyPatch(typeof(AvgUiControl), "OnSceneJumpStop")]
    class OnSceneJumpStopPatch
    {
        static void Postfix()
        {
            SceneJumpSpeedState.Restore();
        }
    }
}
