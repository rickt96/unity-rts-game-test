using UnityEngine;
using Tactical.Units;
using Tactical.Cover;

namespace Tactical.DebugTools
{
    // Assegna manualmente un'unita' a uno slot di un CoverPoint all'avvio,
    // per testare CoverEffectivenessCalculator/ExposureAccumulator prima
    // che esista l'ordine Take Cover (step successivo).
    [RequireComponent(typeof(UnitController))]
    public class DebugAssignCoverSlot : MonoBehaviour
    {
        [SerializeField] private CoverPoint coverPoint;
        [SerializeField] private int slotIndex;
        [SerializeField] private bool isStacked;

        private void Start()
        {
            if (coverPoint == null || slotIndex < 0 || slotIndex >= coverPoint.Slots.Count) return;

            var unit = GetComponent<UnitController>();
            var slot = coverPoint.Slots[slotIndex];

            slot.Occupant = unit;
            unit.CurrentCoverSlot = slot;
            unit.IsStacked = isStacked;
        }
    }
}
