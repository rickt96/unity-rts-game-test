using UnityEngine;

namespace Tactical.Combat
{
    // Parametri di bilanciamento globali dipendenti dalla difficolta' scelta
    // dal giocatore. Un solo asset attivo per partita.
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Tactical/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        public string difficultyName;

        [Tooltip("Soglia di esposizione base oltre la quale un soldato muore")]
        public float baseExposureThreshold = 10f;

        [Tooltip("Moltiplicatore applicato alla soglia base (>1 = piu' resistente)")]
        public float exposureThresholdMultiplier = 1f;

        [Tooltip("Raggio massimo (metri) entro cui un'unita' cerca copertura per riflesso di sopravvivenza")]
        public float survivalReflexRadius = 5f;
    }
}
