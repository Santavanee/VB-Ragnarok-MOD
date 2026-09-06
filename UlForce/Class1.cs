using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VBRForceLock
{
    [BepInPlugin("vbr.force.lock", "VBR Force Lock", "3.2.0")]
    public class ForceLockPlugin : BaseUnityPlugin
    {
        internal static bool Enabled = false;

        void Awake()
        {
            Logger.LogInfo("VBR Force Lock loaded");
            new Harmony("vbr.force.lock").PatchAll();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Enabled = !Enabled;
                Logger.LogInfo($"[MAP FORCE] Toggle = {Enabled}");
            }
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
