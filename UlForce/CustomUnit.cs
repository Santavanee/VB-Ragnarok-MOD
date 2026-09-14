using System;
using BepInEx;
using DataSet;
using HarmonyLib;

namespace VBRForceLock
{
    [BepInPlugin("vbr.force.customunit", "VBR Custom Unit", "1.3.0")]
    public class CustomUnitPlugin : BaseUnityPlugin
    {
        // Compatibility cleanup for saves touched by the retired Research experiment.
        private const string LeftoverResearchNodeMarker = "Unit custom dari seri VBR lain.";
        private const string RetiredLuluId = "zzz_custom_lulu";

        // Continue/load replaces userData, so initialize once per object, not once per game.
        private userDataSet _lastProcessedUserData;

        private void Awake()
        {
            Logger.LogInfo("VBR Custom Unit loaded (Lulu dan Mary otomatis masuk roster)");
            CustomUnitPortraits.Load(Logger);
            var harmony = new Harmony("vbr.force.customunit");
            foreach (Type patchType in CustomUnitPortraits.PatchTypes)
            {
                try { harmony.PatchAll(patchType); }
                catch (Exception e)
                {
                    Logger.LogWarning("[CustomUnit] Gagal patch " + patchType.Name + ": " + e);
                }
            }
        }

        private void Update()
        {
            userDataSet userData = GameDatas.Instance != null ? GameDatas.Instance.userData : null;
            if (userData == null || userData == _lastProcessedUserData) return;
            _lastProcessedUserData = userData;

            RemoveLeftoverResearchNode(userData);
            RemoveRetiredLulu(userData);
            AddLulu(userData);
            AddMary(userData);
        }

        private void RemoveRetiredLulu(userDataSet userData)
        {
            int removedFromRoster = 0;
            for (int i = userData.UnitData.player.Count - 1; i >= 0; i--)
            {
                if (userData.UnitData.player[i].id == RetiredLuluId)
                {
                    userData.UnitData.player.RemoveAt(i);
                    removedFromRoster++;
                }
            }

            int removedFromTemplates = 0;
            for (int i = userData.UnitDataSet.Count - 1; i >= 0; i--)
            {
                if (userData.UnitDataSet[i].id == RetiredLuluId)
                {
                    userData.UnitDataSet.RemoveAt(i);
                    removedFromTemplates++;
                }
            }

            if (removedFromRoster > 0 || removedFromTemplates > 0)
            {
                Logger.LogInfo("[CustomUnit] Unit lama 'zzz_custom_lulu' dibuang dari save.");
            }
        }

        private void AddMary(userDataSet userData)
        {
            UnitData existing = userData.UnitData.player.Find(u => u.id == MaryDefinition.Id);
            if (existing != null)
            {
                // Refresh mod data while preserving the player's division, level, and equipment.
                MaryDefinition.ApplyAppearance(existing.unitDatas);
                MaryDefinition.ApplySkills(existing.unitDatas);
                existing.image1[0] = MaryDefinition.PortraitKey;
                existing.image1[4] = MaryDefinition.PortraitKey;
                existing.SetBaseSkill(existing.unitDatas, -1);
                Logger.LogInfo("[CustomUnit] '" + MaryDefinition.Name + "' sudah ada di roster.");
                return;
            }

            UnitDataSet template = userData.UnitDataSet.Find(u => u.id == MaryDefinition.Id);
            if (template == null)
            {
                template = MaryDefinition.CreateTemplate(userData.UnitDataSet);
                userData.UnitDataSet.Add(template);
            }
            userData.UnitData.player.Add(MaryDefinition.CreateUnit(template));
            Logger.LogInfo("[CustomUnit] '" + MaryDefinition.Name + "' ditambahkan ke roster player.");
        }

        private void RemoveLeftoverResearchNode(userDataSet userData)
        {
            for (int i = userData.ResearchMatrixData.Count - 1; i >= 0; i--)
            {
                if (userData.ResearchMatrixData[i].comment == LeftoverResearchNodeMarker)
                {
                    userData.ResearchMatrixData.RemoveAt(i);
                    Logger.LogInfo("[CustomUnit] Node Research bekas mod sebelumnya dibuang.");
                }
            }
        }

        // Refresh saved templates as well as newly created units.
        private void AddLulu(userDataSet userData)
        {
            UnitData existing = userData.UnitData.player.Find(u => u.id == LuluDefinition.Id);
            if (existing != null)
            {
                existing.barrack = 0;
                existing.division = -1;
                // Keep image1[1] unchanged for the battle HUD asset lookup.
                existing.image1[0] = LuluDefinition.PortraitKey;
                existing.image1[4] = LuluDefinition.PortraitKey;

                LuluDefinition.ApplySkills(existing.unitDatas);
                existing.SetBaseSkill(existing.unitDatas, -1);
                LuluDefinition.ApplyCosmetics(existing.unitDatas);
                Logger.LogInfo("[CustomUnit] '" + LuluDefinition.Name + "' sudah ada di roster, dipastikan aktif.");
                return;
            }

            UnitDataSet template = LuluDefinition.CreateTemplate(userData.UnitDataSet);
            userData.UnitDataSet.Add(template);

            UnitData unit = LuluDefinition.CreateUnit(template);
            userData.UnitData.player.Add(unit);
            Logger.LogInfo("[CustomUnit] '" + LuluDefinition.Name + "' ditambahkan ke roster player.");
        }
    }
}
