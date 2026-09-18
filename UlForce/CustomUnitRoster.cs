using System.Collections.Generic;
using DataSet;

namespace VBRForceLock
{
    internal static class CustomUnitRoster
    {
        internal static void Add(List<UnitData> roster, UnitData unit)
        {
            // DivisionData stores this value and reads roster[value] directly.
            // Template indices belong to a separate list and must not be reused.
            unit.index = roster.Count;
            roster.Add(unit);
        }

        internal static int RepairIndices(List<UnitData> roster)
        {
            int repaired = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                if (roster[i].index == i) continue;
                roster[i].index = i;
                repaired++;
            }
            // Division slots already represent roster positions; do not remap them
            // using the old, possibly duplicate template indices.
            return repaired;
        }

        internal static void RemoveAt(List<UnitData> roster, List<DivisionData> divisions, int position)
        {
            // Removing a retired unit shifts every later roster position.
            foreach (DivisionData division in divisions)
            {
                ShiftSlots(division.divs, position);
                ShiftSlots(division.divsBack, position);
            }
            roster.RemoveAt(position);
            RepairIndices(roster);
        }

        private static void ShiftSlots(int[] slots, int removed)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] == removed) slots[i] = -1;
                else if (slots[i] > removed) slots[i]--;
        }
    }
}
