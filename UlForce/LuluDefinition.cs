using System.Collections.Generic;
using DataSet;

namespace VBRForceLock
{
    internal static class LuluDefinition
    {
        internal const string Id = "zzz_custom_lulu2";
        internal const string Name = "Faceless Reaper Lulu";
        internal const string PortraitKey = "zzz_custom_lulu_portrait";
        internal const string PortraitFileName = "UlForce_lulu.png";

        internal static UnitData CreateUnit(UnitDataSet template)
        {
            UnitData unit = new UnitData(template);
            unit.division = -1;
            unit.barrack = 0;
            unit.SetExp(1);
            unit.status.pow = 199;
            unit.status.def = 93;
            unit.status.spd = 172;
            unit.status.wis = 42;
            return unit;
        }

        // Race/slay match Luna Fairy. US loyalty dialogue occupies script slots 5-9.
        internal static void ApplyCosmetics(UnitDataSet template)
        {
            template.tribe = "女魔飛樹夜氷"; // Female, Demon, Fly, Nature, Night, Ice
            template.special = "炎氷雷"; // Fire, Ice, Lightning
            template.comment = "Death doesn't need a face to find you.";

            template.script[5] = "...Don't get in my way.";
            template.script[6] = "Hm. You're still standing. Noted.";
            template.script[7] = "Fine. I'll cut a path for you today.";
            template.script[8] = "Stay close. My scythe won't mistake you for prey.";
            template.script[9] = "Even a faceless reaper... can call this home.";
        }

        internal static UnitDataSet CreateTemplate(List<UnitDataSet> masterList)
        {
            UnitDataSet baseTemplate = masterList[0];
            UnitDataSet custom = (UnitDataSet)baseTemplate.Clone();

            custom.index = GetNextUnitIndex(masterList);
            custom.id = Id;
            custom.name = Name;
            custom.rank = 17;
            custom.cost = 7;
            custom.pay = 2;
            custom.open = HDDataSetDef.MAXOPEN;
            ApplyCosmetics(custom);

            // HP uses ((level - 1) * 0.25 + 1) * basic.hp. Keep the calibrated base stats.
            custom.basic.Set(35, 16, 30, 7, 109);

            // Leave image1[1] intact: the battle HUD needs a real built-in asset key.
            custom.image1[0] = PortraitKey;
            custom.image1[4] = PortraitKey;

            ApplySkills(custom);

            return custom;
        }

        internal static void ApplySkills(UnitDataSet template)
        {
            SetSkillSlot(template.skillBase, 0, "L014", 40); // Lightning Boost
            SetSkillSlot(template.skillBase, 1, "B011", 20); // Lightning Field
            SetSkillSlot(template.skillBase, 2, "I005", 20); // Flank Attack
            SetSkillSlot(template.skillBase, 3, "I003", 0);  // Cross Attack
            SetSkillSlot(template.skillBase, 4, "I010", 50); // Dimension Slash
            SetSkillSlot(template.skillBase, 5, "I011", 50); // Lethal Critical
            SetSkillSlot(template.skillBase, 6, "I015", 50); // Full Power Attack
            SetSkillSlot(template.skillBase, 7, "J002", 0);  // Range Null
            SetSkillSlot(template.leader, 0, "I010", 25);    // Dimension Slash (tier leader)
            SetSkillSlot(template.leader, 1, "L030", 50);    // Squad Boost (tier leader)
            SetSkillSlot(template.trick, 0, "L030", 15);     // Squad Boost (tier trick)
        }

        private static void SetSkillSlot(List<SkillData> targetList, int targetIdx, string skillId, int power)
        {
            if (targetIdx >= targetList.Count)
            {
                return;
            }
            SkillData sk = new SkillData();
            sk.id = skillId;
            // The sentinel makes the game resolve the localized skill name by ID.
            sk.name = HDDataSetDef.NULL;
            sk.power = power;
            targetList[targetIdx] = sk;
        }

        // Avoid LINQ: the game ships an older Mono/System.Core runtime.
        private static int GetNextUnitIndex(List<UnitDataSet> masterList)
        {
            int max = 0;
            for (int i = 0; i < masterList.Count; i++)
            {
                if (masterList[i].index > max)
                {
                    max = masterList[i].index;
                }
            }
            return max + 1;
        }
    }
}
