using UnityEngine;
using UnityEngine.InputSystem;
using Tactical.Cover;
using Tactical.Orders;

namespace Tactical.DebugTools
{
    // Aiuto temporaneo per testare Move To/Take Cover in Play mode prima
    // che esista l'indicatore ad anello (step successivo). Tasto M = Move
    // To verso moveTarget, tasto C = Take Cover sullo slot indicato.
    [RequireComponent(typeof(OrderController))]
    public class DebugOrderIssuer : MonoBehaviour
    {
        [SerializeField] private Transform moveTarget;
        [SerializeField] private CoverPoint coverPoint;
        [SerializeField] private int slotIndex;

        private OrderController orderController;

        private void Awake()
        {
            orderController = GetComponent<OrderController>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.mKey.wasPressedThisFrame && moveTarget != null)
                orderController.IssueMoveTo(moveTarget.position);

            if (keyboard.cKey.wasPressedThisFrame && coverPoint != null)
                orderController.IssueTakeCover(coverPoint, slotIndex);
        }
    }
}
