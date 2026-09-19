using System.Collections.Generic;
using DataSet;
using HarmonyLib;

namespace VBRForceLock
{
    internal static class ElishaDefinition
    {
        internal const string Id = "zzz_custom_abyss_miko_elisha";
        internal const string Name = "Abyss Miko Elisha";
        internal const string PortraitKey = "zzz_custom_elisha_portrait";
        internal const string PortraitFileName = "UlForce_elisha.png";

        internal static UnitDataSet CreateTemplate(List<UnitDataSet> list)
        {
            var template = (UnitDataSet)list[0].Clone();
            int max = 0;
            foreach (var entry in list)
            {
                if (entry.index > max) max = entry.index;
            }
            template.index = max + 1;
            template.id = Id;
            template.rank = 3; // Class-D
            template.cost = 20;
            template.pay = 2; // Mana
            template.open = HDDataSetDef.MAXOPEN;
            // Native growth: base HP 82 scales to ~6211 at Lv 300, matching screenshot HP: 6202.
            // Caster/Miko distribution with high WIS and support defenses.
            template.basic.Set(35, 85, 65, 125, 82);
            template.job = 3; // Staff / Priestess
            template.equipID[0] = 3; // Staff slot (Caduceus)
            template.equipID[1] = 9; // Robe / Coat slot (Admiral Coat)
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
            template.job = 3;
            template.divine[0] = 1; // Aqua / Abyss
            template.divine[1] = 5; // Night / Dark
            template.tribe = "女神死夜超"; // Woman, Divine, Undead, Night, Supreme
            template.special = "神魔死"; // Slay: Divine, Demon, Undead
            template.comment = "A Miko of the underworld who hopes to live in the human world. A portion of the Abyss God's power resides within her, and she possesses the authority to act on behalf of the Abyss God.";
            for (int i = 5; i < 10; i++)
            {
                template.script[i] = "I hope to be of help to everyone.";
            }
            template.image1[0] = PortraitKey;
            template.image1[4] = PortraitKey;

            template.skillBase = new List<SkillData>
            {
                Skill("L011", 40), // Undead Boost (Dom Undead replacement)
                Skill("M011", 20), // Command Undead
                Skill("L019", 20), // Night Boost (Dark Domain replacement)
                Skill("O002", 75), // Strat Support
                Skill("J013", 0),  // Target Miss
                Skill("H014", 0),  // Multi-Ailment (Debilitating replacement)
                Skill("B012", 20), // Poison Field (M-Poison Field)
                Skill("F003", 20)  // Division Heal (Group Heal)
            };

            template.leader = new List<SkillData>
            {
                Skill("M011", 50), // Command Undead
                Skill("J007", 0)   // Surround Null
            };

        }

        internal static void ApplySupportSkills(UnitDataSet template, List<UnitDataSet> masterList)
        {
            string sourceId = "m0671";
            UnitDataSet source = masterList.Find(u => u.id == sourceId);
            if (source == null) throw new System.InvalidOperationException("Missing Elisha skill source: " + sourceId);
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

