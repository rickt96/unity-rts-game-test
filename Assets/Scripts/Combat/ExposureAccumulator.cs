using System.Collections.Generic;
using UnityEngine;
using Tactical.Units;
using Tactical.Cover;

namespace Tactical.Combat
{
    // Accumulo di esposizione al fuoco per singolo soldato (squadra o
    // nemico). Non e' un pool di hit point: la morte scatta al superamento
    // della soglia, non colpo per colpo. E' pero' un accumulo "continuativo"
    // (vedi brief: tempo di esposizione continuativo), non un totale a vita:
    // quando il fuoco pesato e' sotto exposureDecayPerSecond (copertura
    // buona o nessuna minaccia attiva) l'esposizione decade verso 0 invece
    // di restare bloccata al valore raggiunto. SquadSuppressionMultiplier
    // viene sincronizzato automaticamente dal SquadSuppressionTracker della
    // squadra (se presente) ad ogni frame; resta comunque settabile
    // manualmente per i test in isolamento senza una squadra assegnata.
    [RequireComponent(typeof(UnitController))]
    [RequireComponent(typeof(UnitStateMachine))]
    public class ExposureAccumulator : MonoBehaviour
    {
        [SerializeField] private DifficultyConfig difficultyConfig;
        [SerializeField] private CoverEffectivenessSettings coverSettings;
        [SerializeField] private float exposureDecayPerSecond = 1f;
        [SerializeField] private bool enableDeathOnExposure = false;

        private UnitController unit;
        private UnitStateMachine stateMachine;
        private readonly List<ThreatSource> activeThreats = new List<ThreatSource>();

        public float CurrentExposure { get; private set; }
        public float SquadSuppressionMultiplier { get; set; } = 1f;

        // Letti dal riflesso di sopravvivenza: fuoco pesato ricevuto questo
        // frame (post-copertura, come nell'accumulo) e il piu' urgente
        // (piu' corto) periodo di grazia tra le minacce attive, per capire
        // se e quanto a lungo l'unita' e' sotto tiro sostenuto.
        public float CurrentWeightedFire { get; private set; }
        public float MinThreatGracePeriod { get; private set; }
        public IReadOnlyList<ThreatSource> ActiveThreats => activeThreats;

        private void Awake()
        {
            unit = GetComponent<UnitController>();
            stateMachine = GetComponent<UnitStateMachine>();
        }

        public void AddThreat(Transform attacker, WeaponConfig weapon)
        {
            RemoveThreat(attacker);
            activeThreats.Add(new ThreatSource(attacker, weapon));
        }

        public void RemoveThreat(Transform attacker)
        {
            activeThreats.RemoveAll(t => t.Attacker == attacker);
        }

        private void Update()
        {
            if (stateMachine.CurrentState == UnitState.Dead) return;

            if (unit.Squad != null && unit.Squad.TryGetComponent(out SquadSuppressionTracker squadTracker))
                SquadSuppressionMultiplier = squadTracker.ExposureMultiplier;

            if (difficultyConfig == null) return;

            float weightedFire = 0f;
            float minGracePeriod = float.PositiveInfinity;
            foreach (var threat in activeThreats)
            {
                if (threat.Attacker == null || threat.Weapon == null) continue;

                float effectiveness = ComputeCoverEffectivenessAgainst(threat.Attacker.position);
                weightedFire += threat.Weapon.exposureWeight * (1f - effectiveness);

                if (threat.Weapon.survivalReflexGracePeriod < minGracePeriod)
                    minGracePeriod = threat.Weapon.survivalReflexGracePeriod;
            }
            CurrentWeightedFire = weightedFire;
            MinThreatGracePeriod = float.IsPositiveInfinity(minGracePeriod) ? 0f : minGracePeriod;

            float net = weightedFire * SquadSuppressionMultiplier - exposureDecayPerSecond;
            CurrentExposure = Mathf.Max(0f, CurrentExposure + net * Time.deltaTime);

            float deathThreshold = difficultyConfig.baseExposureThreshold * difficultyConfig.exposureThresholdMultiplier;
            if (enableDeathOnExposure && CurrentExposure >= deathThreshold)
            {
                stateMachine.ChangeState(UnitState.Dead);
            }
        }

        private float ComputeCoverEffectivenessAgainst(Vector3 shooterPosition)
        {
            if (!unit.IsInCover || coverSettings == null) return 0f;

            return CoverEffectivenessCalculator.Evaluate(
                shooterPosition,
                transform.position,
                unit.CurrentCoverSlot.Normal,
                unit.CurrentCoverSlot.Height,
                unit.IsStacked,
                coverSettings);
        }
    }
}
