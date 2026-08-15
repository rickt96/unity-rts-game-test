using System.Collections.Generic;
using UnityEngine;
using Tactical.Units;
using Tactical.Cover;

namespace Tactical.Combat
{
    // Soppressione aggregata a livello di squadra (non per singolo
    // soldato): un attaccante "ingaggia" la squadra come bersaglio, il che
    // somma il suo peso alla soppressione di gruppo. La singola minaccia
    // pero' non colpisce tutti i membri contemporaneamente: si concentra
    // sempre sul membro con la priorita' di bersaglio piu' alta verso
    // quell'attaccante (esposizione E distanza: piu' vicino ed esposto,
    // piu' attira il fuoco), ricalcolata a intervalli regolari - cosi' se
    // la squadra si mette in copertura e uno resta scoperto (o in stack),
    // il fuoco si sposta su di lui invece di restare spalmato su tutti.
    [RequireComponent(typeof(Squad))]
    public class SquadSuppressionTracker : MonoBehaviour
    {
        [SerializeField] private CoverEffectivenessSettings coverSettings;
        [SerializeField] private float suppressionDecayPerSecond = 2f;
        [SerializeField] private float movementSuppressionThreshold = 5f;
        [SerializeField] private float maxSuppression = 10f;
        [SerializeField] private float suppressionToExposureMultiplierScale = 0.1f;

        [Tooltip("Ogni quanti secondi si ricalcola il bersaglio piu' esposto per ogni minaccia attiva: tenuto sopra 0 per non farlo ogni singolo frame quando ci sono molte unita'/nemici.")]
        [SerializeField] private float retargetInterval = 0.15f;

        private float retargetTimer;

        // Registro dei tracker attivi: usato dal debug visualizer per
        // disegnare a chi sta sparando ogni nemico senza wiring manuale.
        public static readonly List<SquadSuppressionTracker> All = new List<SquadSuppressionTracker>();

        private Squad squad;
        private readonly List<ThreatSource> activeThreats = new List<ThreatSource>();
        private readonly Dictionary<Transform, UnitController> currentTargets = new Dictionary<Transform, UnitController>();

        public float CurrentSuppression { get; private set; }
        public bool IsSuppressed => CurrentSuppression >= movementSuppressionThreshold;

        // Per ogni attaccante attivo, il membro che sta attualmente
        // colpendo (letto dal debug visualizer per disegnare il vettore).
        public IReadOnlyDictionary<Transform, UnitController> CurrentTargets => currentTargets;

        // Moltiplicatore applicato all'accumulo di esposizione dei membri
        // colpiti: piu' la squadra e' soppressa, piu' velocemente sale
        // l'esposizione di chi viene effettivamente raggiunto dal fuoco.
        public float ExposureMultiplier => 1f + CurrentSuppression * suppressionToExposureMultiplierScale;

        private void Awake()
        {
            squad = GetComponent<Squad>();
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        public void EngageThreat(Transform attacker, WeaponConfig weapon)
        {
            activeThreats.RemoveAll(t => t.Attacker == attacker);
            var threat = new ThreatSource(attacker, weapon);
            activeThreats.Add(threat);

            AssignThreatToMostExposedMember(threat);
        }

        public void DisengageThreat(Transform attacker)
        {
            activeThreats.RemoveAll(t => t.Attacker == attacker);
            currentTargets.Remove(attacker);

            foreach (var member in squad.Members)
            {
                if (member != null && member.TryGetComponent(out ExposureAccumulator accumulator))
                    accumulator.RemoveThreat(attacker);
            }
        }

        private void Update()
        {
            float weightedFire = 0f;
            foreach (var threat in activeThreats)
            {
                if (threat.Attacker == null || threat.Weapon == null) continue;
                weightedFire += threat.Weapon.suppressionWeight;
            }

            // Ricalcolato a intervalli (non ogni frame): se il membro
            // colpito si copre, si allontana, o un altro diventa piu'
            // esposto/vicino, l'attaccante si ri-concentra su di lui.
            retargetTimer -= Time.deltaTime;
            if (retargetTimer <= 0f)
            {
                retargetTimer = retargetInterval;
                foreach (var threat in activeThreats)
                {
                    if (threat.Attacker == null || threat.Weapon == null) continue;
                    AssignThreatToMostExposedMember(threat);
                }
            }

            CurrentSuppression = Mathf.Clamp(
                CurrentSuppression + (weightedFire - suppressionDecayPerSecond) * Time.deltaTime,
                0f, maxSuppression);
        }

        private void AssignThreatToMostExposedMember(ThreatSource threat)
        {
            if (threat.Attacker == null || threat.Weapon == null) return;

            UnitController target = FindMostExposedMember(threat);
            if (target != null)
                currentTargets[threat.Attacker] = target;
            else
                currentTargets.Remove(threat.Attacker);

            foreach (var member in squad.Members)
            {
                if (member == null || !member.TryGetComponent(out ExposureAccumulator accumulator)) continue;

                if (member == target)
                    accumulator.AddThreat(threat.Attacker, threat.Weapon);
                else
                    accumulator.RemoveThreat(threat.Attacker);
            }
        }

        // Priorita' di bersaglio: combina esposizione (1 - efficacia
        // copertura) e vicinanza. A parita' di copertura, chi e' piu'
        // vicino all'attaccante attira piu' fuoco; un membro ben coperto
        // (efficacia 1) ha sempre priorita' 0, indipendentemente dalla
        // distanza. Il termine (1 + distanza) evita la divisione per zero
        // e degrada la priorita' in modo continuo con la distanza, senza
        // mai azzerarla del tutto.
        private UnitController FindMostExposedMember(ThreatSource threat)
        {
            UnitController best = null;
            float bestPriority = float.NegativeInfinity;

            foreach (var member in squad.Members)
            {
                if (member == null) continue;

                float effectiveness = ComputeEffectiveness(member, threat);
                float distance = Vector3.Distance(threat.Attacker.position, member.transform.position);
                float priority = (1f - effectiveness) / (1f + distance);

                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    best = member;
                }
            }

            return best;
        }

        // Stessa CoverEffectivenessCalculator usata da ExposureAccumulator
        // e dall'anteprima dell'indicatore ad anello: un membro scoperto
        // (o senza coverSettings assegnato) e' considerato massimamente
        // esposto (efficacia 0), cosi' l'attaccante lo preferisce sempre a
        // uno in copertura.
        private float ComputeEffectiveness(UnitController member, ThreatSource threat)
        {
            if (!member.IsInCover || coverSettings == null) return 0f;

            return CoverEffectivenessCalculator.Evaluate(
                threat.Attacker.position,
                member.transform.position,
                member.CurrentCoverSlot.Normal,
                member.CurrentCoverSlot.Height,
                member.IsStacked,
                coverSettings);
        }
    }
}
