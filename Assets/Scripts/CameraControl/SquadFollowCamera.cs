using UnityEngine;
using UnityEngine.InputSystem;
using Tactical.CommandUI;

namespace Tactical.CameraControl
{
    // Camera in terza persona agganciata al membro selezionato (tramite
    // SquadSelector: Tab cambia squadra, 1-4 cambiano membro dentro la
    // squadra corrente): orbita attorno a lui ad una distanza fissa in
    // base al movimento del mouse (yaw/pitch), cosi' il giocatore vede
    // quello che vede il soldato invece di una vista dall'alto, come da
    // design. Il look-around si attiva solo tenendo premuto il tasto
    // destro: il cursore resta altrimenti libero, perche' serve visibile e
    // assoluto all'indicatore ad anello per puntare il terreno.
    // fallbackTarget copre il caso senza SquadSelector assegnato (test in
    // isolamento).
    public class SquadFollowCamera : MonoBehaviour
    {
        [SerializeField] private SquadSelector squadSelector;
        [SerializeField] private Transform fallbackTarget;
        [SerializeField] private float distance = 5f;
        [SerializeField] private float lookHeightOffset = 1.4f;
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float initialPitch = 15f;

        private float yaw;
        private float pitch;
        private Transform trackedTarget;

        private Transform CurrentTarget
        {
            get
            {
                var member = squadSelector != null ? squadSelector.CurrentMember : null;
                return member != null ? member.transform : fallbackTarget;
            }
        }

        private void OnEnable()
        {
            pitch = initialPitch;
            trackedTarget = CurrentTarget;
            yaw = trackedTarget != null ? trackedTarget.eulerAngles.y : 0f;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (mouse.rightButton.wasReleasedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (!mouse.rightButton.isPressed) return;

            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * mouseSensitivity;
            pitch -= delta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private void LateUpdate()
        {
            var target = CurrentTarget;
            if (target == null) return;

            if (target != trackedTarget)
            {
                trackedTarget = target;
                yaw = target.eulerAngles.y;
                pitch = initialPitch;
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * lookHeightOffset;

            transform.position = pivot - rotation * Vector3.forward * distance;
            transform.rotation = rotation;
        }
    }
}
