using System.Collections.Generic;
using DataSet;
using HarmonyLib;

namespace VBRForceLock
{
    internal static class AnoraDefinition
    {
        internal const string Id = "zzz_custom_white_maiden_anora";
        internal const string Name = "White Maiden Anora";
        internal const string PortraitKey = "zzz_custom_anora_portrait";
        internal const string PortraitFileName = "UlForce_anora.png";

        internal static UnitDataSet CreateTemplate(List<UnitDataSet> list)
        {
            var template = (UnitDataSet)list[0].Clone();
            int max = 0;
            foreach (var entry in list) if (entry.index > max) max = entry.index;
            template.index = max + 1;
            template.id = Id;
            template.rank = 17;
            template.cost = 17;
            template.pay = 2;
            template.open = HDDataSetDef.MAXOPEN;
            template.basic.Set(20, 110, 45, 85, 115);
            template.equipID[0] = 3;
            template.equipID[1] = 9;
            Apply(template);
            ApplySupportSkills(template, list);
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

        internal static void Apply(UnitDataSet template)
        {
            var names = Traverse.Create(template).Field("_name");
            names.SetValue(new List<string>(names.GetValue<List<string>>()));
            template.name = Name;
            template.type = "英霊";
            template.tribe = "女魔神";
            template.special = HDDataSetDef.NULL;
            template.comment = "Anora, a pure white maiden who stays back from the front line. She waits joyfully for the one she loves.";
            for (int i = 5; i < 10; i++) template.script[i] = "Ahh, these calm days are the best...";
            template.image1[0] = PortraitKey;
            template.image1[4] = PortraitKey;
            template.skillBase = new List<SkillData>
            {
                Skill("J011", 0), // Defense Only
                Skill("R003", 50), // Treasure Hunt
                Skill("D003", 50), // Barrier
                Skill("J016", 95), // Tiny Physique
                Skill("J015", 100), // Godly Physique
                Skill("L032", 150), // Defense Formation (Group DEF replacement)
                Skill("L005", 100), // Divine Boost
                Skill("M030", 15) // Command Division
            };
            template.leader = new List<SkillData> { new SkillData(), new SkillData() };
        }

        internal static void ApplySupportSkills(UnitDataSet template, List<UnitDataSet> masterList)
        {
            string sourceId = "m0698"; // Eternal Promise Anora (Wedding Vows, Plasma Barrier, Wraith Wave, Second Chance, Loched Fate; Barrier)
            UnitDataSet source = masterList.Find(u => u.id == sourceId);
            if (source == null) throw new System.InvalidOperationException("Missing Anora skill source: " + sourceId);
            template.trick = new List<SkillData>();
            foreach (SkillData skill in source.trick)
                template.trick.Add((SkillData)skill.Clone());
            template.tactics = new List<TacticsData>();
            foreach (TacticsData tactic in source.tactics)
                template.tactics.Add((TacticsData)tactic.Clone());
        }

        internal static void RefreshSupportSkills(UnitData unit)
        {
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
