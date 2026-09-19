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
            Logger.LogInfo("VBR Custom Unit loaded (Lulu, Mary, Nanna, Anora, Miden, Elisha, Loki otomatis masuk roster)");
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
            AddAnora(userData);
            AddMiden(userData);
            AddElisha(userData);
            AddGoldenLoki(userData);
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

        private void AddGoldenLoki(userDataSet userData)
        {
            var template = userData.UnitDataSet.Find(u => u.id == GoldenLokiDefinition.Id);
            if (template != null)
            {
                template.open = HDDataSetDef.MAXOPEN;
            }

            var existing = userData.UnitData.player.Find(u => u.id == GoldenLokiDefinition.Id);
            if (existing != null)
            {
                if (existing.unitDatas != null)
                {
                    existing.unitDatas.open = HDDataSetDef.MAXOPEN;
                }

                if (existing.barrack < 0)
                {
                    GoldenLokiDefinition.UnlockUnit(existing);
                    Logger.LogInfo("[CustomUnit] '" + GoldenLokiDefinition.Name + "' di-unlock dan masuk ke roster player (Level 1).");
                }
                else if (existing.level == 175)
                {
                    GoldenLokiDefinition.ResetToLevel1(existing);
                    Logger.LogInfo("[CustomUnit] '" + GoldenLokiDefinition.Name + "' direset ke Level 1 tanpa equipment.");
                }
                else
                {
                    Logger.LogInfo("[CustomUnit] '" + GoldenLokiDefinition.Name + "' sudah ada di roster (Level " + existing.level + ").");
                }
                return;
            }

            if (template == null)
            {
                Logger.LogWarning("[CustomUnit] Template tidak ditemukan: " + GoldenLokiDefinition.Id);
                return;
            }

            UnitData unit = GoldenLokiDefinition.CreateUnit(template);
            CustomUnitRoster.Add(userData.UnitData.player, unit);
            Logger.LogInfo("[CustomUnit] '" + GoldenLokiDefinition.Name + "' ditambahkan ke roster player (Level 1).");
        }

        private void AddAnora(userDataSet userData)
        {
            var template = userData.UnitDataSet.Find(u => u.id == AnoraDefinition.Id);
            if (template == null)
            {
                template = AnoraDefinition.CreateTemplate(userData.UnitDataSet);
                userData.UnitDataSet.Add(template);
            }
            else
            {
                AnoraDefinition.Apply(template);
                AnoraDefinition.ApplySupportSkills(template, userData.UnitDataSet);
            }
            var existing = userData.UnitData.player.Find(u => u.id == AnoraDefinition.Id);
            if (existing != null)
            {
                AnoraDefinition.Apply(existing.unitDatas);
                AnoraDefinition.ApplySupportSkills(existing.unitDatas, userData.UnitDataSet);
                AnoraDefinition.RefreshSupportSkills(existing);
                existing.image1[0] = AnoraDefinition.PortraitKey;
                existing.image1[4] = AnoraDefinition.PortraitKey;
                existing.SetBaseSkill(existing.unitDatas, -1);
            }
            else CustomUnitRoster.Add(userData.UnitData.player, AnoraDefinition.CreateUnit(template));
            Logger.LogInfo("[CustomUnit] White Maiden Anora siap di roster.");
        }

        private void AddMiden(userDataSet userData)
        {
            var template = userData.UnitDataSet.Find(u => u.id == MidenDefinition.Id);
            if (template == null)
            {
                template = MidenDefinition.CreateTemplate(userData.UnitDataSet);
                userData.UnitDataSet.Add(template);
            }
            else
            {
                MidenDefinition.Apply(template);
                MidenDefinition.ApplySupportSkills(template, userData.UnitDataSet);
            }
            var existing = userData.UnitData.player.Find(u => u.id == MidenDefinition.Id);
            if (existing != null)
            {
                MidenDefinition.Apply(existing.unitDatas);
                MidenDefinition.ApplySupportSkills(existing.unitDatas, userData.UnitDataSet);
                MidenDefinition.RefreshSupportSkills(existing);
                existing.image1[0] = MidenDefinition.PortraitKey;
                existing.image1[4] = MidenDefinition.PortraitKey;
                existing.SetBaseSkill(existing.unitDatas, -1);
            }
            else CustomUnitRoster.Add(userData.UnitData.player, MidenDefinition.CreateUnit(template));
            Logger.LogInfo("[CustomUnit] Twilight Miko Miden siap di roster.");
        }

        private void AddElisha(userDataSet userData)
        {
            var template = userData.UnitDataSet.Find(u => u.id == ElishaDefinition.Id);
            if (template == null)
            {
                template = ElishaDefinition.CreateTemplate(userData.UnitDataSet);
                userData.UnitDataSet.Add(template);
            }
            else
            {
                ElishaDefinition.Apply(template);
                ElishaDefinition.ApplySupportSkills(template, userData.UnitDataSet);
            }
            var existing = userData.UnitData.player.Find(u => u.id == ElishaDefinition.Id);
            if (existing != null)
            {
                ElishaDefinition.Apply(existing.unitDatas);
                ElishaDefinition.ApplySupportSkills(existing.unitDatas, userData.UnitDataSet);
                ElishaDefinition.RefreshSupportSkills(existing);
                existing.image1[0] = ElishaDefinition.PortraitKey;
                existing.image1[4] = ElishaDefinition.PortraitKey;
                existing.SetBaseSkill(existing.unitDatas, -1);
            }
            else CustomUnitRoster.Add(userData.UnitData.player, ElishaDefinition.CreateUnit(template));
            Logger.LogInfo("[CustomUnit] Abyss Miko Elisha siap di roster.");
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
            var template = userData.UnitDataSet.Find(u => u.id == MaryDefinition.Id);
            if (template == null)
            {
                template = MaryDefinition.CreateTemplate(userData.UnitDataSet);
                userData.UnitDataSet.Add(template);
            }
            else
            {
                MaryDefinition.ApplyAppearance(template);
                MaryDefinition.ApplySkills(template);
                MaryDefinition.ApplySupportSkills(template, userData.UnitDataSet);
            }
            var existing = userData.UnitData.player.Find(u => u.id == MaryDefinition.Id);
            if (existing != null)
            {
                // Refresh mod data while preserving the player's division, level, and equipment.
                MaryDefinition.ApplyAppearance(existing.unitDatas);
                MaryDefinition.ApplySkills(existing.unitDatas);
                MaryDefinition.ApplySupportSkills(existing.unitDatas, userData.UnitDataSet);
                MaryDefinition.RefreshSupportSkills(existing);
                existing.image1[0] = MaryDefinition.PortraitKey;
                existing.image1[4] = MaryDefinition.PortraitKey;
                existing.SetBaseSkill(existing.unitDatas, -1);
                Logger.LogInfo("[CustomUnit] '" + MaryDefinition.Name + "' sudah ada di roster.");
                return;
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
