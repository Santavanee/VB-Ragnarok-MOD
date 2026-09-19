using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VBRForceLock
{
    [BepInPlugin("vbr.force.lock", "VBR Force Lock", "3.3.0")]
    public class ForceLockPlugin : BaseUnityPlugin
    {
        internal static bool Enabled = false;

        void Awake()
        {
            Logger.LogInfo("VBR Force Lock loaded");
            var harmony = new Harmony("vbr.force.lock");
            foreach (var patch in new[] { typeof(MapForcePatch), typeof(ItemLimitPatch) })
            {
                try { harmony.PatchAll(patch); }
                catch (System.Exception e) { Logger.LogError(e); }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
                RefillResources();

            if (Input.GetKeyDown(KeyCode.F1))
            {
                Enabled = !Enabled;
                Logger.LogInfo($"[MAP FORCE] Toggle = {Enabled}");
            }
        }

        private void RefillResources()
        {
            var userData = GameDatas.Instance != null ? GameDatas.Instance.userData : null;
            if (userData == null || userData.forceData == null)
            {
                Logger.LogInfo("[Resources] Load permainan dulu sebelum menekan F2.");
                return;
            }

            var resources = userData.forceData.resource;
            if (resources == null || resources.Count < 4 ||
                resources[0] == null || resources[1] == null ||
                resources[2] == null || resources[3] == null)
            {
                Logger.LogWarning("[Resources] Data resource belum siap.");
                return;
            }

            const int target = 900000;
            for (int i = 0; i < 4; i++)
            {
                // The native getter clamps the balance to its storage limit.
                resources[i].limit = System.Math.Max(resources[i].limit, target);
                resources[i].now = target;
            }
            Logger.LogInfo("[Resources] F2: keempat resource diatur ke 900000.");
        }
    }

    [HarmonyPatch(typeof(DataSet.DivisionData), "get_force")]
    class MapForcePatch
    {
        static void Postfix(ref int __result)
        {
            if (!ForceLockPlugin.Enabled)
                return;

            // jangan log di sini (rawan freeze)
            __result = 500;
        }
    }
    // PATCH ITEM LIMIT
    [HarmonyPatch(typeof(DataSet.ItemDataSet), "get_limit")]
    class ItemLimitPatch
    {
        static void Postfix(ref int __result)
        {
            if (__result < 100)
                __result = 100;
        }
    }

}
