using UnityEngine;
using UnityEngine.AI;
using Tactical.Units;

namespace Tactical.Presentation
{
    // Ponte tra la logica di gioco (NavMeshAgent, FSM) e l'Animator del
    // modello visivo. Per ora aggiorna solo il parametro "Speed" in base
    // alla velocita' reale dell'agente (locomozione Idle/Walk). E' il
    // punto in cui agganciare in futuro gli altri trigger di animazione
    // (mira, sparo, entrata in copertura, ferimento, morte) agli stati
    // gia' esistenti in UnitStateMachine, senza toccare la logica di
    // combattimento - esattamente come previsto dal brief.
    [RequireComponent(typeof(NavMeshAgent))]
    public class CharacterAnimationDriver : MonoBehaviour
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        [SerializeField] private Animator animator;
        [SerializeField] private UnitStateMachine stateMachine;

        private NavMeshAgent agent;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (stateMachine == null) stateMachine = GetComponent<UnitStateMachine>();
        }

        private void OnEnable()
        {
            if (stateMachine != null) stateMachine.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (stateMachine != null) stateMachine.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            if (animator == null) return;
            animator.SetFloat(SpeedParam, agent.velocity.magnitude);
        }

        private void HandleStateChanged(UnitState previous, UnitState next)
        {
            // Aggancio futuro: altri trigger (Aiming, Firing, EnteringCover,
            // Suppressed, Wounded, Dead) quando saranno disponibili le
            // animazioni corrispondenti e i relativi stati nel controller.
        }
    }
}
