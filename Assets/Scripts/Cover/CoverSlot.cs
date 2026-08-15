using System.Collections.Generic;
using UnityEngine;
using Tactical.Units;

namespace Tactical.Cover
{
    public enum CoverHeight
    {
        Low,
        High
    }

    // Una posizione fisica lungo il fronte di un CoverPoint. La normale del
    // transform indica la direzione verso cui la copertura protegge.
    [System.Serializable]
    public class CoverSlot
    {
        [SerializeField] private Transform slotTransform;
        [SerializeField] private CoverHeight height = CoverHeight.Low;

        public Vector3 Position => slotTransform.position;
        public Vector3 Normal => slotTransform.forward;
        public CoverHeight Height => height;

        [System.NonSerialized] public UnitController Occupant;
        [System.NonSerialized] public readonly List<UnitController> Stack = new List<UnitController>();

        public bool IsFree => Occupant == null;
    }
}
