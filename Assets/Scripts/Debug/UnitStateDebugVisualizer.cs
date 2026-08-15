using UnityEngine;
using Tactical.Units;

namespace Tactical.DebugTools
{
    // Colora il renderer del soldato (capsula di default) in base allo stato
    // corrente della FSM, per validare le transizioni prima che esistano le
    // animazioni vere. Usa _BaseColor perche' il progetto e' su URP.
    [RequireComponent(typeof(UnitStateMachine))]
    [RequireComponent(typeof(Renderer))]
    public class UnitStateDebugVisualizer : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private UnitStateMachine stateMachine;
        private Renderer unitRenderer;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            stateMachine = GetComponent<UnitStateMachine>();
            unitRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            stateMachine.OnStateChanged += HandleStateChanged;
            ApplyColor(stateMachine.CurrentState);
        }

        private void OnDisable()
        {
            stateMachine.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(UnitState previous, UnitState next)
        {
            ApplyColor(next);
        }

        private void ApplyColor(UnitState state)
        {
            unitRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, ColorForState(state));
            unitRenderer.SetPropertyBlock(propertyBlock);
        }

        private static Color ColorForState(UnitState state)
        {
            switch (state)
            {
                case UnitState.Idle: return Color.white;
                case UnitState.Moving: return Color.blue;
                case UnitState.EnteringCover: return Color.cyan;
                case UnitState.InCover: return Color.green;
                case UnitState.Aiming: return new Color(1f, 0.5f, 0f);
                case UnitState.Firing: return Color.red;
                case UnitState.Suppressed: return Color.yellow;
                case UnitState.SurvivalReflex: return Color.magenta;
                case UnitState.Wounded: return new Color(0.5f, 0f, 0f);
                case UnitState.Dead: return Color.black;
                default: return Color.gray;
            }
        }
    }
}
