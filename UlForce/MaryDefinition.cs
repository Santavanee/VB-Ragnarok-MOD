using System.Collections.Generic;
using DataSet;

namespace VBRForceLock
{
    internal static class MaryDefinition
    {
        internal const string Id = "zzz_custom_swimsuit_mary";
        internal const string Name = "Swimsuit Queen Mary";
        internal const string PortraitKey = "zzz_custom_mary_portrait";
        internal const string PortraitFileName = "UlForce_mary.png";

        internal static UnitDataSet CreateTemplate(List<UnitDataSet> masterList)
        {
            // Clone a real template to retain the game's required list sizes and defaults.
            UnitDataSet template = (UnitDataSet)masterList[0].Clone();
            int maxIndex = 0;
            foreach (UnitDataSet entry in masterList)
                if (entry.index > maxIndex) maxIndex = entry.index;
            template.index = maxIndex + 1;
            template.id = Id;
            template.name = Name;
            template.rank = 17;
            template.cost = 12;
            template.pay = 2;
            template.open = HDDataSetDef.MAXOPEN;
            // Approximate screenshot stats under the game's native EXP scaling.
            // HP at level 106: floor(27.25 * 110) = 2997.
            template.basic.Set(119, 90, 48, 8, 110);
            template.equipID[0] = 1; // Spear slot, left empty.
            template.equipID[1] = 9; // Robe slot, left empty.
            ApplyAppearance(template);
            ApplySkills(template);
            ApplySupportSkills(template, masterList);
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

        internal static void ApplyAppearance(UnitDataSet template)
        {
            // Female, Human, Aqua, Supreme; slay icons include Mechanical twice.
            template.tribe = "女人海超";
            template.special = "海魔器超器";
            template.comment = "Mary has become the swimsuit pirate queen after receiving Theofrad's gift. Her new looks inspire her mateys in a whole new way.";
            for (int i = 5; i < 10; i++)
                template.script[i] = "I'll show you the power of the pirate ruler!";
            template.image1[0] = PortraitKey;
            template.image1[4] = PortraitKey;
            // image1[1] must remain a built-in asset key for battle tags.
        }

        internal static void ApplySkills(UnitDataSet template)
        {
            template.skillBase = new List<SkillData>
            {
                Skill("I007", 200), // Added Attack
                Skill("R004", 14),  // Bounty Hunter (game reward table only supports levels 1-14).
                Skill("I011", 75), // Lethal Critical
                Skill("I012", 80), // Helmet Split: 50 + 30 (both white entries).
                Skill("I005", 5),  // Flank Attack
                Skill("I016", 60), // Counter Resist (excludes equipment's +25).
                Skill("J008", 80), // Slayer Defense
                Skill("R003", 56)  // Treasure Hunt: 36 + 20 (excludes equipment's +6).
            };
            template.leader = new List<SkillData>
            {
                Skill("O002", 150), // Strat Support
                Skill("R004", 8)    // Bounty Hunter (leader only; no equipment's +4).
            };
        }

        internal static void ApplySupportSkills(UnitDataSet template, List<UnitDataSet> masterList)
        {
            string sourceId = "m0655"; // Hel (Storm Javelin, Thunder Lance, Mist Blade, Sea Storm Stone, Blikjandabol; AddedAtk)
            UnitDataSet source = masterList.Find(u => u.id == sourceId);
            if (source == null) throw new System.InvalidOperationException("Missing Mary skill source: " + sourceId);
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
