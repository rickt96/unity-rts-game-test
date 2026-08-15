using UnityEngine;
using UnityEngine.InputSystem;
using Tactical.Units;

namespace Tactical.DebugTools
{
    // Aiuto temporaneo per forzare gli stati della FSM in Play mode e
    // validarne il "feel" prima che esista il sistema ordini (step
    // successivo). Da rimuovere una volta disponibili gli ordini reali.
    [RequireComponent(typeof(UnitStateMachine))]
    public class UnitStateDebugInput : MonoBehaviour
    {
        private static readonly Key[] Keys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
            Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0
        };

        private static readonly UnitState[] Cycle =
        {
            UnitState.Idle,
            UnitState.Moving,
            UnitState.EnteringCover,
            UnitState.InCover,
            UnitState.Aiming,
            UnitState.Firing,
            UnitState.Suppressed,
            UnitState.SurvivalReflex,
            UnitState.Wounded,
            UnitState.Dead
        };

        private UnitStateMachine stateMachine;

        private void Awake()
        {
            stateMachine = GetComponent<UnitStateMachine>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            for (int i = 0; i < Keys.Length; i++)
            {
                if (keyboard[Keys[i]].wasPressedThisFrame)
                {
                    stateMachine.ChangeState(Cycle[i]);
                }
            }
        }
    }
}
