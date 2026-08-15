using System.Collections.Generic;
using Tactical.Units;

namespace Tactical.Cover
{
    // Assegna un gruppo di soldati a un CoverPoint: chi rientra negli slot
    // liberi li occupa normalmente, gli eccedenti si accodano in stack
    // dietro il primo slot. Non muta lo stato del CoverPoint: puo' essere
    // chiamata liberamente per l'anteprima dell'indicatore ad anello e poi
    // di nuovo per l'ordine reale, senza effetti collaterali.
    public static class CoverSlotAssigner
    {
        public readonly struct Assignment
        {
            public readonly UnitController Unit;
            public readonly CoverSlot Slot;
            public readonly bool IsStacked;

            public Assignment(UnitController unit, CoverSlot slot, bool isStacked)
            {
                Unit = unit;
                Slot = slot;
                IsStacked = isStacked;
            }
        }

        // Restituisce null se la copertura non ha capacita' sufficiente per
        // l'intero gruppo: l'ordine va rifiutato in blocco, non in parte.
        public static List<Assignment> TryAssign(CoverPoint coverPoint, IReadOnlyList<UnitController> units)
        {
            if (coverPoint == null || units == null || units.Count == 0) return null;
            if (coverPoint.Slots.Count == 0) return null;
            if (!coverPoint.HasCapacityFor(units.Count)) return null;

            var freeSlots = new List<CoverSlot>();
            foreach (var slot in coverPoint.Slots)
                if (slot.IsFree) freeSlots.Add(slot);

            CoverSlot stackSlot = coverPoint.Slots[0];
            var result = new List<Assignment>(units.Count);
            int freeIndex = 0;

            foreach (var unit in units)
            {
                if (freeIndex < freeSlots.Count)
                {
                    result.Add(new Assignment(unit, freeSlots[freeIndex], false));
                    freeIndex++;
                }
                else
                {
                    result.Add(new Assignment(unit, stackSlot, true));
                }
            }

            return result;
        }
    }
}
