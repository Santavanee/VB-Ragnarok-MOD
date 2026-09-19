using DataSet;

namespace VBRForceLock
{
    internal static class GoldenLokiDefinition
    {
        internal const string Id = "m1009";
        internal const string Name = "Golden Overlord Loki";

        internal static UnitData CreateUnit(UnitDataSet template)
        {
            UnitData unit = new UnitData(template);
            unit.division = -1;
            unit.barrack = 0;
            unit.standby = 0;
            unit.RemoveEquipAll();
            unit.SetExp(1);
            unit.loyalty = 100;
            unit.valor = 100;
            if (unit.unitDatas != null)
            {
                unit.SetBaseSkill(unit.unitDatas, -1);
            }
            unit.GetStatus();
            unit.hp.now = unit.hp.max;
            return unit;
        }

        internal static void UnlockUnit(UnitData unit)
        {
            if (unit == null) return;
            unit.barrack = 0;
            unit.standby = 0;
            unit.division = -1;
            unit.loyalty = 100;
            unit.valor = 100;
            unit.RemoveEquipAll();
            unit.SetExp(1);
            if (unit.unitDatas != null)
            {
                unit.SetBaseSkill(unit.unitDatas, -1);
            }
            unit.GetStatus();
            unit.hp.now = unit.hp.max;
        }

        internal static void ResetToLevel1(UnitData unit)
        {
            if (unit == null) return;
            unit.RemoveEquipAll();
            unit.SetExp(1);
            if (unit.unitDatas != null)
            {
                unit.SetBaseSkill(unit.unitDatas, -1);
            }
            unit.GetStatus();
            unit.hp.now = unit.hp.max;
        }
    }
}
