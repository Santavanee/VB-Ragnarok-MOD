using System;
using BepInEx;
using DataSet;
using HarmonyLib;

namespace VBRForceLock
{
    [BepInPlugin("vbr.force.customunit", "VBR Custom Unit", "1.4.1")]
    public class CustomUnitPlugin : BaseUnityPlugin
    {
        // Compatibility cleanup for saves touched by the retired Research experiment.
        private const string LeftoverResearchNodeMarker = "Unit custom dari seri VBR lain.";
        private const string RetiredLuluId = "zzz_custom_lulu";

        // Continue/load replaces userData, so initialize once per object, not once per game.
        private userDataSet _lastProcessedUserData;

        private void Awake()
        {
            Logger.LogInfo("VBR Custom Unit loaded (Lulu, Mary, Celestial Nanna, Eclipse Nanna otomatis masuk roster)");
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
            int repaired = CustomUnitRoster.RepairIndices(userData.UnitData.player);
            if (repaired > 0)
                Logger.LogInfo("[CustomUnit] Memperbaiki index roster " + repaired + " unit agar sesuai posisi Division.");
            AddLulu(userData);
            AddMary(userData);
            AddNanna(userData, false);
            AddNanna(userData, true);
            foreach (UnitDataSet template in userData.UnitDataSet)
                if (IsModUnit(template.id)) template.type = "英霊";
            foreach (UnitData unit in userData.UnitData.player)
                if (IsModUnit(unit.id))
                {
                    unit.type = "英霊";
                    unit.unitDatas.type = "英霊";
                }
        }

        private static bool IsModUnit(string id)
        {
            return id != null && id.StartsWith("zzz_custom_", StringComparison.Ordinal);
        }

        private void AddNanna(userDataSet userData, bool dark)
        {
            string id = NannaDefinition.Id(dark);
            UnitDataSet template = userData.UnitDataSet.Find(u => u.id == id);
            if (template == null)
            {
                template = NannaDefinition.CreateTemplate(userData.UnitDataSet, dark);
                userData.UnitDataSet.Add(template);
            }
            else NannaDefinition.Apply(template, dark);
            NannaDefinition.ApplySupportSkills(template, userData.UnitDataSet, dark);

            UnitData existing = userData.UnitData.player.Find(u => u.id == id);
            if (existing != null)
            {
                // Preserve division, barrack, EXP, equipment, and player progression on reload.
                NannaDefinition.Apply(existing.unitDatas, dark);
                NannaDefinition.ApplySupportSkills(existing.unitDatas, userData.UnitDataSet, dark);
                NannaDefinition.RefreshSupportSkills(existing);
                existing.image1[0] = NannaDefinition.PortraitKey(dark);
                existing.image1[4] = NannaDefinition.PortraitKey(dark);
                existing.SetBaseSkill(existing.unitDatas, -1);
                Logger.LogInfo("[CustomUnit] '" + NannaDefinition.Name(dark) + "' sudah ada di roster.");
                return;
            }
            CustomUnitRoster.Add(userData.UnitData.player, NannaDefinition.CreateUnit(template));
            Logger.LogInfo("[CustomUnit] '" + NannaDefinition.Name(dark) + "' ditambahkan ke roster player.");
        }

        private void RemoveRetiredLulu(userDataSet userData)
        {
            int removedFromRoster = 0;
            for (int i = userData.UnitData.player.Count - 1; i >= 0; i--)
            {
                if (userData.UnitData.player[i].id == RetiredLuluId)
                {
                    CustomUnitRoster.RemoveAt(userData.UnitData.player, userData.Divisions.player, i);
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
            CustomUnitRoster.Add(userData.UnitData.player, MaryDefinition.CreateUnit(template));
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
                // Preserve the player's existing division and barrack assignment.
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
            CustomUnitRoster.Add(userData.UnitData.player, unit);
            Logger.LogInfo("[CustomUnit] '" + LuluDefinition.Name + "' ditambahkan ke roster player.");
        }
    }
}
