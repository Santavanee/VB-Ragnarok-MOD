using System.Collections.Generic;
using DataSet;
using HarmonyLib;

namespace VBRForceLock
{
    internal static class MidenDefinition
    {
        internal const string Id = "zzz_custom_twilight_miko_miden";
        internal const string Name = "Twilight Miko Miden";
        internal const string PortraitKey = "zzz_custom_miden_portrait";
        internal const string PortraitFileName = "UlForce_miden.png";

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
            template.rank = 3; // D-Class
            template.cost = 26;
            template.pay = 2;
            template.open = HDDataSetDef.MAXOPEN;
            // Native growth: at Lv 19 with Estranged Stars (+28 POW, +12 SPD, +8 WIS)
            // and Vermillion Cape (+36 DEF, +12 WIS), stats reach 180 / 118 / 102 / 39, HP 660.
            template.basic.Set(134, 72, 79, 15, 120);
            template.job = 2; // Archer / Bow
            template.equipID[0] = 2; // Bow slot
            template.equipID[1] = 9; // Robe / Cape slot
            Apply(template);
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
            template.job = 2;
            template.divine[0] = 5; // Moon / Night
            template.divine[1] = -1;
            template.tribe = "女器神超"; // Woman, Mechanical, Divine, Supreme
            template.special = "全"; // Slay: All (∞)
            template.comment = "An Embryo girl calling herself the Miko of Twilight. Behind her calm and kind demeanor lies a dangerous portent of the end.";
            for (int i = 5; i < 10; i++)
            {
                template.script[i] = "Now, the curtain closes.";
            }
            template.image1[0] = PortraitKey;
            template.image1[4] = PortraitKey;

            template.skillBase = new List<SkillData>
            {
                Skill("I004", 0),   // All Attack
                Skill("I015", 80),  // Full Power Attack / Max-Power Attack
                Skill("I010", 40),  // Dimension Slash
                Skill("I005", 15),  // Flank Attack
                Skill("J013", 0),   // Target Miss
                Skill("E002", 70),  // Spell Barrier
                Skill("J023", 100), // S-Destruct Wall
                Skill("B013", 20)   // Light Field
            };

            template.leader = new List<SkillData>
            {
                Skill("M010", 20), // Command Mech
                Skill("H001", 3)   // Fool's Lie
            };

            for (int i = 0; i < template.trick.Count; i++) template.trick[i] = new SkillData();
            for (int i = 0; i < template.tactics.Count; i++) template.tactics[i] = new TacticsData();
        }

        private static SkillData Skill(string id, int power)
        {
            return new SkillData { id = id, name = HDDataSetDef.NULL, power = power };
        }
    }
}

