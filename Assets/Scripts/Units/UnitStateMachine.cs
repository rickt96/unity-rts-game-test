using System;
using UnityEngine;

namespace Tactical.Units
{
    // Stato centralizzato del soldato. I trigger di animazione (fase
    // successiva) si agganciano a OnStateChanged senza toccare la logica
    // di combattimento. Contiene solo le guardie di transizione note oggi;
    // il sistema ordini (step successivo) imposta ReflexSuppressedByOrder.
    public class UnitStateMachine : MonoBehaviour
    {
        [SerializeField] private UnitState currentState = UnitState.Idle;

        public bool ReflexSuppressedByOrder { get; set; }

        public UnitState CurrentState => currentState;
        public event Action<UnitState, UnitState> OnStateChanged;

        public bool ChangeState(UnitState newState)
        {
            if (!CanEnterState(newState)) return false;

            UnitState previous = currentState;
            currentState = newState;
            OnStateChanged?.Invoke(previous, newState);
            return true;
        }

        private bool CanEnterState(UnitState newState)
        {
            if (currentState == UnitState.Dead) return false;
            if (currentState == newState) return false;

            if (newState == UnitState.SurvivalReflex)
            {
                // Il riflesso non interrompe un movimento ordinato (Move To
                // deve concludersi o la squadra tornare Idle) ed e' soppresso
                // durante ordini Assault/Suppress.
                if (currentState == UnitState.Moving) return false;
                if (ReflexSuppressedByOrder) return false;
            }

            return true;
        }
    }
}
