using UnityEngine;
using Tactical.Units;
using Tactical.Cover;
using Tactical.Combat;

namespace Tactical.Orders
{
    // Riflesso di sopravvivenza (simmetrico squadra/nemici): quando l'unita'
    // e' ferma (Idle/InCover/Suppressed, mai durante un movimento ordinato
    // - gia' garantito dal guard della FSM) e sotto un volume di fuoco
    // pesato sopra soglia per un periodo di grazia sostenuto (il piu' corto
    // tra le armi che la stanno colpendo), cerca la copertura libera piu'
    // efficace entro un raggio limitato attorno a se', riusando la stessa
    // CoverEffectivenessCalculator dell'accumulo di esposizione e
    // dell'anteprima dell'indicatore ad anello. E' un riflesso locale e
    // temporaneo: non trovando nulla di meglio entro il raggio, non fa
    // nulla. Una volta scattato, la nuova posizione diventa la baseline
    // (nessun ritorno automatico alla posizione precedente).
    [RequireComponent(typeof(UnitController))]
    [RequireComponent(typeof(UnitStateMachine))]
    [RequireComponent(typeof(ExposureAccumulator))]
    [RequireComponent(typeof(OrderController))]
    public class SurvivalReflexController : MonoBehaviour
    {
        [SerializeField] private bool reflexEnabled = false; // per ora spento
        [SerializeField] private DifficultyConfig difficultyConfig;
        [SerializeField] private CoverEffectivenessSettings coverSettings;
        [SerializeField] private float weightedFireThreshold = 1.2f;

        private UnitController unit;
        private UnitStateMachine stateMachine;
        private ExposureAccumulator exposureAccumulator;
        private OrderController orderController;

        private float sustainedThreatTimer;

        private void Awake()
        {
            unit = GetComponent<UnitController>();
            stateMachine = GetComponent<UnitStateMachine>();
            exposureAccumulator = GetComponent<ExposureAccumulator>();
            orderController = GetComponent<OrderController>();
        }

        private void Update()
        {
            if (!reflexEnabled) return;

            bool isThreatened = exposureAccumulator.CurrentWeightedFire >= weightedFireThreshold;
            sustainedThreatTimer = isThreatened ? sustainedThreatTimer + Time.deltaTime : 0f;

            var state = stateMachine.CurrentState;
            if (state != UnitState.Idle && state != UnitState.InCover && state != UnitState.Suppressed)
                return;

            if (!isThreatened || sustainedThreatTimer < exposureAccumulator.MinThreatGracePeriod)
                return;

            CoverSlot bestSlot = FindBestCoverSlot();
            if (bestSlot == null) return;

            sustainedThreatTimer = 0f;

            // Beat di reazione a se' stante (per il futuro trigger di
            // animazione "tuffo in copertura"), poi l'effettivo spostamento
            // e' delegato a OrderController: stessa strada di un ordine
            // Take Cover qualsiasi, nessuna logica di movimento duplicata.
            stateMachine.ChangeState(UnitState.SurvivalReflex);
            orderController.IssueTakeCoverAssignment(bestSlot, false);
        }

        private CoverSlot FindBestCoverSlot()
        {
            float radius = difficultyConfig != null ? difficultyConfig.survivalReflexRadius : 5f;
            Vector3 position = transform.position;

            CoverSlot best = null;
            float bestWeightedFire = float.PositiveInfinity;

            foreach (var coverPoint in CoverPoint.All)
            {
                foreach (var slot in coverPoint.Slots)
                {
                    if (!slot.IsFree) continue;
                    if (Vector3.Distance(position, slot.Position) > radius) continue;

                    float score = ScoreSlotAgainstActiveThreats(slot);
                    if (score < bestWeightedFire)
                    {
                        bestWeightedFire = score;
                        best = slot;
                    }
                }
            }

            return best;
        }

        // Stessa funzione di valutazione copertura usata dall'accumulo di
        // esposizione, applicata pero' a un punto ipotetico (lo slot
        // candidato) invece che alla posizione attuale del soldato.
        private float ScoreSlotAgainstActiveThreats(CoverSlot slot)
        {
            if (coverSettings == null) return float.PositiveInfinity;

            float weightedFire = 0f;
            foreach (var threat in exposureAccumulator.ActiveThreats)
            {
                if (threat.Attacker == null || threat.Weapon == null) continue;

                float effectiveness = CoverEffectivenessCalculator.Evaluate(
                    threat.Attacker.position,
                    slot.Position,
                    slot.Normal,
                    slot.Height,
                    false,
                    coverSettings);

                weightedFire += threat.Weapon.exposureWeight * (1f - effectiveness);
            }

            return weightedFire;
        }
    }
}
