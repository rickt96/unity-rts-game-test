using UnityEngine;

namespace Tactical.Cover
{
    // Parametri geometrici del calcolo di efficacia copertura, esposti come
    // asset per essere bilanciati senza toccare il codice.
    [CreateAssetMenu(fileName = "CoverEffectivenessSettings", menuName = "Tactical/Cover Effectiveness Settings")]
    public class CoverEffectivenessSettings : ScriptableObject
    {
        [Header("Angolo orizzontale")]
        [Tooltip("Angolo orizzontale (gradi) dalla normale entro cui la copertura protegge al 100%, a prescindere dall'altezza")]
        public float horizontalMaxProtectedAngle = 30f;
        [Tooltip("Gradi oltre la soglia massima in cui la protezione orizzontale decade linearmente a 0")]
        public float horizontalFalloffRange = 60f;

        [Header("Copertura bassa")]
        [Tooltip("Angolo di elevazione (gradi) entro cui la copertura bassa protegge al 100%")]
        public float lowCoverMaxProtectedElevationAngle = 15f;
        [Tooltip("Gradi oltre la soglia massima in cui la protezione decade linearmente a 0")]
        public float lowCoverElevationFalloffRange = 25f;

        [Header("Copertura alta")]
        [Tooltip("Angolo di elevazione (gradi) entro cui la copertura alta protegge al 100%")]
        public float highCoverMaxProtectedElevationAngle = 40f;
        [Tooltip("Gradi oltre la soglia massima in cui la protezione decade linearmente a 0")]
        public float highCoverElevationFalloffRange = 30f;

        [Header("Stacking")]
        [Tooltip("Esponente applicato all'efficacia per i soldati in stack: a frontalita' piena (fattore 1) non cambia nulla, ma amplifica la riduzione su angoli laterali/alti")]
        public float stackPenaltyExponent = 2f;
    }
}
