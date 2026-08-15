using UnityEngine;
using Tactical.Combat;

namespace Tactical.DebugTools
{
    // Simula un tiratore che spara continuativamente su un bersaglio, per
    // testare CoverEffectivenessCalculator ed ExposureAccumulator in
    // isolamento (1 vs 1) prima che esista il combattimento reale (ordini
    // Suppress/Attack e IA nemica, step successivi). Abilita/disabilita
    // l'oggetto in Play mode per avviare/fermare il fuoco simulato.
    public class DebugThreatEmitter : MonoBehaviour
    {
        [SerializeField] private ExposureAccumulator target;
        [SerializeField] private WeaponConfig weapon;

        private void OnEnable()
        {
            if (target != null && weapon != null)
                target.AddThreat(transform, weapon);
        }

        private void OnDisable()
        {
            if (target != null)
                target.RemoveThreat(transform);
        }
    }
}
