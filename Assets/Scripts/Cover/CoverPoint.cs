using System.Collections.Generic;
using UnityEngine;
using Tactical.Units;

namespace Tactical.Cover
{
    // Un fronte di copertura fisico piazzato a mano nel livello (o istanza
    // di uno dei tipi predefiniti: muretto base 1 slot/stack 2, muretto
    // medio 2 slot/stack 2, muretto lungo 4 slot/stack 0, muro alto come
    // base ma altezza High). maxStackCount e' un numero separato dagli
    // slot: la capacita' totale e' sempre slot + stack.
    public class CoverPoint : MonoBehaviour
    {
        [SerializeField] private List<CoverSlot> slots = new List<CoverSlot>();
        [SerializeField] private int maxStackCount = 2;

        // Registro di tutti i CoverPoint attivi in scena: evita di dover
        // dare un collider a ciascuno solo per farli trovare dall'indicatore
        // ad anello (che ha gia' un raycast contro il terreno per il punto
        // di riferimento) o, in futuro, dal riflesso di sopravvivenza.
        public static readonly List<CoverPoint> All = new List<CoverPoint>();

        public IReadOnlyList<CoverSlot> Slots => slots;
        public int MaxStackCount => maxStackCount;
        public int MaxTotalCapacity => slots.Count + maxStackCount;

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        public int CurrentOccupancy
        {
            get
            {
                int count = 0;
                foreach (var slot in slots)
                {
                    if (!slot.IsFree) count++;
                    count += slot.Stack.Count;
                }
                return count;
            }
        }

        public bool HasCapacityFor(int unitCount) => CurrentOccupancy + unitCount <= MaxTotalCapacity;
    }
}
