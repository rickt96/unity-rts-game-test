using UnityEngine;
using Tactical.Combat;

namespace Tactical.DebugTools
{
    // Mostra a schermo il valore di esposizione corrente, per validare a
    // occhio la formula durante i test 1 vs 1 (angolo/altezza/stack).
    public class ExposureDebugDisplay : MonoBehaviour
    {
        [SerializeField] private ExposureAccumulator accumulator;

        private void OnGUI()
        {
            if (accumulator == null || Camera.main == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(accumulator.transform.position + Vector3.up * 2f);
            if (screenPos.z < 0f) return;

            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 220, 20),
                $"Esposizione: {accumulator.CurrentExposure:F2}");
        }
    }
}
