using UnityEngine;
using Tactical.Combat;

namespace Tactical.DebugTools
{
    // Come DebugThreatEmitter ma ingaggia un'intera squadra tramite
    // SquadSuppressionTracker.EngageThreat, per testare il volume di fuoco
    // multi-nemico: piu' istanze puntate alla stessa squadra sommano
    // soppressione ed espongono individualmente ogni membro.
    public class DebugSquadThreatEmitter : MonoBehaviour
    {
        [SerializeField] private SquadSuppressionTracker targetSquad;
        [SerializeField] private WeaponConfig weapon;

        private void OnEnable()
        {
            if (targetSquad != null && weapon != null)
                targetSquad.EngageThreat(transform, weapon);
        }

        private void OnDisable()
        {
            if (targetSquad != null)
                targetSquad.DisengageThreat(transform);
        }
    }
}
