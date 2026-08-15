using UnityEngine;
using Tactical.Cover;

namespace Tactical.Units
{
    public enum SoldierRole
    {
        TeamLeader,
        AutomaticRifleman,
        Grenadier,
        Rifleman
    }

    // Scheletro dati di un soldato (squadra o nemico). Esposizione/
    // soppressione e riflesso di sopravvivenza sono componenti separati
    // aggiunti nelle fasi successive; la FSM di stato e' gia' presente.
    [RequireComponent(typeof(UnitStateMachine))]
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private SoldierRole role;
        [SerializeField] private Squad squad;

        public SoldierRole Role => role;
        public Squad Squad => squad;

        [System.NonSerialized] public CoverSlot CurrentCoverSlot;
        [System.NonSerialized] public bool IsStacked;

        public bool IsInCover => CurrentCoverSlot != null;

        public UnitStateMachine StateMachine { get; private set; }

        private void Awake()
        {
            StateMachine = GetComponent<UnitStateMachine>();
        }
    }
}
