using System.Collections.Generic;
using DataSet;
using HarmonyLib;

namespace VBRForceLock
{
    internal static class NannaDefinition
    {
        internal const string LightId = "zzz_custom_celestial_nanna";
        internal const string DarkId = "zzz_custom_eclipse_nanna";
        internal static string Id(bool dark) { return dark ? DarkId : LightId; }
        internal static string Name(bool dark) { return dark ? "Eclipse Nanna" : "Celestial Nanna"; }
        internal static string PortraitKey(bool dark) { return dark ? "zzz_custom_eclipse_nanna_portrait" : "zzz_custom_celestial_nanna_portrait"; }
        internal static string PortraitFileName(bool dark) { return dark ? "UlForce_nanna_dark.png" : "UlForce_nanna.png"; }

        internal static UnitDataSet CreateTemplate(List<UnitDataSet> masterList, bool dark)
        {
            UnitDataSet template = (UnitDataSet)masterList[0].Clone();
            int maxIndex = 0;
            foreach (UnitDataSet entry in masterList)
                if (entry.index > maxIndex) maxIndex = entry.index;
            template.index = maxIndex + 1;
            template.id = Id(dark);
            template.rank = 17;
            template.cost = 17;
            template.pay = 2;
            template.open = HDDataSetDef.MAXOPEN;
            // Native growth, not forced UI stats. HP at level 112: floor(28.75 * 115) = 3306.
            template.basic.Set(dark ? 152 : 142, dark ? 84 : 94, dark ? 72 : 67, 34, 115);
            template.equipID[0] = 1; // Two-handed weapon; equipment is left to the player.
            template.equipID[1] = 9; // Robe / cloak.
            Apply(template, dark);
            ApplySupportSkills(template, masterList, dark);
            return template;
        }

        internal static UnitData CreateUnit(UnitDataSet template)
        {
            UnitData unit = new UnitData(template);
            unit.division = -1;
            unit.barrack = 0;
            unit.SetExp(1);
            unit.loyalty = 100;
            unit.valor = 100;
            unit.GetStatus();
            unit.hp.now = unit.hp.max;
            return unit;
        }

        internal static void Apply(UnitDataSet template, bool dark)
        {
            // Clone() leaves the localized name list shared with its source.
            var names = Traverse.Create(template).Field("_name");
            names.SetValue(new List<string>(names.GetValue<List<string>>()));
            template.name = Name(dark);
            template.type = "英霊"; // HeroicSpirit
            template.divine[0] = dark ? 5 : 4;
            template.divine[1] = -1;
            template.tribe = dark ? "女神魔飛夜超" : "女神飛雷騎超";
            template.special = dark ? "神人飛樹" : "魔死飛毒夜";
            template.comment = dark
                ? "When the heavens fell silent, Nanna crowned herself beneath an eclipse. Her black wings shelter the forsaken, while her blade drinks the strength of those who threaten them."
                : "Nanna bears the crown of the heavens with a gentle heart. Her radiant blade sweeps away darkness, while divine light heals and protects the companions at her side.";
            for (int i = 5; i < 10; i++)
                template.script[i] = dark ? "Even without the sun, I will be your goddess." : "Do I feel like a chief goddess now?";
            template.image1[0] = PortraitKey(dark);
            template.image1[4] = PortraitKey(dark);
            // Keep native image1[1] for battle tag resource loading.
            template.skillBase = new List<SkillData>
            {
                Skill("I004", 0),                 // All Attack
                Skill(dark ? "B014" : "B013", 12), // Dark / Light Field
                Skill(dark ? "L004" : "L005", 25), // Demon / Divine Boost
                Skill("I017", 4),                 // Multi-Attack
                Skill("I010", 25),                // Dimension Slash
                Skill("J015", 60),                // Godly Physique
                Skill(dark ? "I007" : "F004", dark ? 200 : 12), // Added Attack / Equitable Heal
                Skill(dark ? "I005" : "G005", dark ? 90 : 0)  // Flank Attack / Absolute Cure
            };
            template.leader = new List<SkillData>
            {
                Skill(dark ? "M004" : "M005", 15), // Command Demon / Divine
                Skill("O004", 100)                // Strat Barrier
            };
        }

        internal static void ApplySupportSkills(UnitDataSet template, List<UnitDataSet> masterList, bool dark)
        {
            // Keep native tactic IDs and metadata so costs, effects, and localization resolve normally.
            string sourceId = dark ? "m1028" : "m1069";
            UnitDataSet source = masterList.Find(u => u.id == sourceId);
            if (source == null) throw new System.InvalidOperationException("Missing Nanna skill source: " + sourceId);
            template.trick = new List<SkillData>();
            foreach (SkillData skill in source.trick)
                template.trick.Add((SkillData)skill.Clone());
            template.tactics = new List<TacticsData>();
            foreach (TacticsData tactic in source.tactics)
                template.tactics.Add((TacticsData)tactic.Clone());
        }

        internal static void RefreshSupportSkills(UnitData unit)
        {
            // SetBaseSkill refreshes passive effects but does not copy these instance lists.
            unit.trick = new List<SkillData>();
            foreach (SkillData skill in unit.unitDatas.trick)
                unit.trick.Add((SkillData)skill.Clone());
            unit.tactics = new List<TacticsData>();
            foreach (TacticsData tactic in unit.unitDatas.tactics)
                unit.tactics.Add((TacticsData)tactic.Clone());
        }

        private static SkillData Skill(string id, int power)
        {
            return new SkillData { id = id, name = HDDataSetDef.NULL, power = power };
        }
    }
}
