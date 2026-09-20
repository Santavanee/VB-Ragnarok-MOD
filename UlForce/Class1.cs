using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VBRForceLock
{
    [BepInPlugin("vbr.force.lock", "VBR Force Lock", "3.3.0")]
    public class ForceLockPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Logger.LogInfo("VBR Force Lock loaded");
            var harmony = new Harmony("vbr.force.lock");
            try { harmony.PatchAll(typeof(ItemLimitPatch)); }
            catch (System.Exception e) { Logger.LogError(e); }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
                RefillResources();

            if (Input.GetKeyDown(KeyCode.F1))
                RefillForce();
        }

        private void RefillForce()
        {
            var userData = GameDatas.Instance != null ? GameDatas.Instance.userData : null;
            if (userData == null || userData.Divisions == null || userData.Divisions.player == null)
            {
                Logger.LogInfo("[Force] Load permainan dulu sebelum menekan F1.");
                return;
            }

            int count = 0;
            foreach (var div in userData.Divisions.player)
            {
                if (div != null)
                {
                    div.force = 500;
                    count++;
                }
            }

            // Jika sedang berada di battle, perbarui juga divisi pemain dan UI gauge battle
            try
            {
                var bc = Object.FindObjectOfType<BattleSystem.BattleControl>();
                if (bc != null && bc.bc != null)
                {
                    if (bc.bc.battleDivision != null && bc.bc.battleDivision.Count > 0 && bc.bc.battleDivision[0] != null)
                    {
                        bc.bc.battleDivision[0].force = 500;
                    }
                    var forceOld = Traverse.Create(bc.bc).Field<System.Collections.Generic.List<int>>("forceOld").Value;
                    if (forceOld != null && forceOld.Count > 0)
                    {
                        forceOld[0] = 500;
                    }
                }

                var fvc = Object.FindObjectOfType<BattleSystem.FieldValueControl>();
                if (fvc != null)
                {
                    if (fvc.force != null && fvc.force.Count > 0 && fvc.force[0] != null)
                        fvc.force[0].text = "5";
                    if (fvc.gauge != null && fvc.gauge.Count > 0 && fvc.gauge[0] != null)
                        fvc.gauge[0].fillAmount = 0f;
                    fvc.ForceP = 0;
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogWarning($"[Force] Battle UI update: {ex.Message}");
            }

            // Refresh tampilan UI map / division list jika sedang terbuka
            try
            {
                foreach (var info in Object.FindObjectsOfType<DivisionInfoHandler>())
                {
                    info.OnCallUpdareDraw();
                }
                foreach (var list in Object.FindObjectsOfType<DivisionlistHandler>())
                {
                    list.OnCallUpdareDraw();
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogWarning($"[Force] Map UI update: {ex.Message}");
            }

            Logger.LogInfo($"[Force] F1: Force untuk {count} divisi pemain diisi penuh (500).");
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
