using UnityEngine;
using UnityEngine.AI;
using Tactical.Units;
using Tactical.Cover;
using Tactical.Combat;

namespace Tactical.Orders
{
    // Esegue gli ordini base (Move To, Take Cover a slot singolo) pilotando
    // la FSM. Il movimento e' delegato a NavMeshAgent (le coperture sono
    // solide: la NavMesh viene generata a runtime da NavMeshBootstrapper,
    // vedi quello script per i dettagli), cosi' i soldati aggirano gli
    // ostacoli invece di attraversarli in linea retta.
    [RequireComponent(typeof(UnitController))]
    [RequireComponent(typeof(UnitStateMachine))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class OrderController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float arrivalThreshold = 0.15f;
        [SerializeField] private float enteringCoverDuration = 0.4f;
        [SerializeField] private float stackOffsetDistance = 0.6f;

        private UnitController unit;
        private UnitStateMachine stateMachine;
        private NavMeshAgent agent;

        public OrderType CurrentOrder { get; private set; } = OrderType.None;

        private CoverSlot pendingCoverSlot;
        private bool pendingIsStacked;
        private float enteringCoverTimer;

        private void Awake()
        {
            unit = GetComponent<UnitController>();
            stateMachine = GetComponent<UnitStateMachine>();
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
            agent.stoppingDistance = arrivalThreshold;
            agent.updateRotation = true;

            // I soldati non devono essere solidi tra loro: con l'evitamento
            // attivo, agganciarsi a slot/stack ravvicinati dietro una
            // copertura li fa bloccare a vicenda invece di raggiungere la
            // posizione assegnata. Le coperture restano solide (geometria
            // statica della NavMesh), solo l'evitamento agente-agente e'
            // disattivato.
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        public void IssueMoveTo(Vector3 target)
        {
            if (stateMachine.CurrentState == UnitState.Dead) return;

            ReleaseCurrentCover();

            CurrentOrder = OrderType.MoveTo;
            pendingCoverSlot = null;
            agent.SetDestination(target);
            stateMachine.ChangeState(UnitState.Moving);
        }

        // Ordine diretto a uno slot specifico, sempre come occupante proprio
        // (usato dai test manuali senza gruppo/stacking).
        public void IssueTakeCover(CoverPoint coverPoint, int slotIndex)
        {
            if (coverPoint == null || slotIndex < 0 || slotIndex >= coverPoint.Slots.Count) return;
            IssueTakeCoverAssignment(coverPoint.Slots[slotIndex], false);
        }

        // Ordine risultante da CoverSlotAssigner: puo' assegnare uno slot
        // proprio o accodare l'unita' in stack dietro uno slot occupato.
        public void IssueTakeCoverAssignment(CoverSlot slot, bool asStack)
        {
            if (stateMachine.CurrentState == UnitState.Dead) return;
            if (slot == null) return;
            if (!asStack && !slot.IsFree) return;

            ReleaseCurrentCover();

            Vector3 target;
            if (asStack)
            {
                slot.Stack.Add(unit);
                // Ogni soldato in coda si accoda un passo piu' indietro del
                // precedente (stackOffsetDistance per posizione), cosi' non
                // si sovrappongono tutti sullo stesso punto dietro lo slot.
                int stackPosition = slot.Stack.Count;
                target = slot.Position - slot.Normal * (stackOffsetDistance * stackPosition);
            }
            else
            {
                slot.Occupant = unit;
                target = slot.Position;
            }

            pendingCoverSlot = slot;
            pendingIsStacked = asStack;
            agent.SetDestination(target);
            CurrentOrder = OrderType.TakeCover;
            stateMachine.ChangeState(UnitState.Moving);
        }

        private void Update()
        {
            if (stateMachine.CurrentState == UnitState.Dead) return;

            switch (stateMachine.CurrentState)
            {
                case UnitState.Moving:
                    TickMovement();
                    return;
                case UnitState.EnteringCover:
                    TickEnteringCover();
                    return;
            }

            TickSuppressionReaction();
        }

        private bool TryGetSquadTracker(out SquadSuppressionTracker tracker)
        {
            tracker = null;
            return unit.Squad != null && unit.Squad.TryGetComponent(out tracker);
        }

        private void TickSuppressionReaction()
        {
            var state = stateMachine.CurrentState;
            if (state != UnitState.Idle && state != UnitState.InCover && state != UnitState.Suppressed)
                return;

            bool isSquadSuppressed = TryGetSquadTracker(out var tracker) && tracker.IsSuppressed;

            if (isSquadSuppressed && state != UnitState.Suppressed)
                stateMachine.ChangeState(UnitState.Suppressed);
            else if (!isSquadSuppressed && state == UnitState.Suppressed)
                stateMachine.ChangeState(unit.IsInCover ? UnitState.InCover : UnitState.Idle);
        }

        private void TickMovement()
        {
            if (agent.pathPending) return;
            if (agent.remainingDistance > agent.stoppingDistance) return;

            OnArrived();
        }

        private void OnArrived()
        {
            if (CurrentOrder == OrderType.TakeCover && pendingCoverSlot != null)
            {
                enteringCoverTimer = enteringCoverDuration;
                stateMachine.ChangeState(UnitState.EnteringCover);
            }
            else
            {
                CurrentOrder = OrderType.None;
                stateMachine.ChangeState(UnitState.Idle);
            }
        }

        private void TickEnteringCover()
        {
            enteringCoverTimer -= Time.deltaTime;
            if (enteringCoverTimer > 0f) return;

            transform.rotation = Quaternion.LookRotation(pendingCoverSlot.Normal);

            unit.CurrentCoverSlot = pendingCoverSlot;
            unit.IsStacked = pendingIsStacked;
            pendingCoverSlot = null;
            CurrentOrder = OrderType.None;
            stateMachine.ChangeState(UnitState.InCover);
        }

        private void ReleaseCurrentCover()
        {
            if (unit.CurrentCoverSlot != null)
            {
                if (unit.IsStacked)
                    unit.CurrentCoverSlot.Stack.Remove(unit);
                else if (unit.CurrentCoverSlot.Occupant == unit)
                    unit.CurrentCoverSlot.Occupant = null;

                unit.CurrentCoverSlot = null;
                unit.IsStacked = false;
            }

            if (pendingCoverSlot != null)
            {
                if (pendingIsStacked)
                    pendingCoverSlot.Stack.Remove(unit);
                else if (pendingCoverSlot.Occupant == unit)
                    pendingCoverSlot.Occupant = null;

                pendingCoverSlot = null;
                pendingIsStacked = false;
            }
        }
    }
}
