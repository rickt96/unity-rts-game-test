using UnityEngine;

namespace Tactical.Cover
{
    // Funzione di calcolo condivisa da: accumulo esposizione runtime,
    // anteprima dell'indicatore ad anello e riflesso di sopravvivenza
    // (fasi successive). Non tiene stato: dati in, fattore 0..1 fuori.
    public static class CoverEffectivenessCalculator
    {
        public static float Evaluate(
            Vector3 shooterPosition,
            Vector3 defenderPosition,
            Vector3 coverNormal,
            CoverHeight coverHeight,
            bool isStacked,
            CoverEffectivenessSettings settings)
        {
            Vector3 toShooter = shooterPosition - defenderPosition;

            float horizontalFactor = ComputeHorizontalFactor(toShooter, coverNormal, settings);
            float verticalFactor = ComputeVerticalFactor(toShooter, coverHeight, settings);

            float effectiveness = horizontalFactor * verticalFactor;

            if (isStacked)
                effectiveness = Mathf.Pow(effectiveness, settings.stackPenaltyExponent);

            return Mathf.Clamp01(effectiveness);
        }

        // Angolo tra la direzione verso il tiratore e la normale della
        // copertura, proiettati sul piano orizzontale: entro
        // horizontalMaxProtectedAngle la protezione resta piena (un
        // tiratore "davanti" non deve essere allineato al grado esatto),
        // oltre decade linearmente a 0 nel range di falloff. Stesso
        // modello a plateau usato per il fattore verticale, cosi' un
        // nemico leggermente disassato non fa piu' trapelare esposizione
        // residua all'infinito.
        private static float ComputeHorizontalFactor(Vector3 toShooter, Vector3 coverNormal, CoverEffectivenessSettings settings)
        {
            Vector3 toShooterXZ = new Vector3(toShooter.x, 0f, toShooter.z);
            Vector3 normalXZ = new Vector3(coverNormal.x, 0f, coverNormal.z);

            if (toShooterXZ.sqrMagnitude < 0.0001f || normalXZ.sqrMagnitude < 0.0001f)
                return 1f;

            float cosAngle = Vector3.Dot(toShooterXZ.normalized, normalXZ.normalized);
            float angle = Mathf.Acos(Mathf.Clamp(cosAngle, -1f, 1f)) * Mathf.Rad2Deg;

            if (angle <= settings.horizontalMaxProtectedAngle) return 1f;
            if (settings.horizontalFalloffRange <= 0f || angle >= settings.horizontalMaxProtectedAngle + settings.horizontalFalloffRange)
                return 0f;

            return 1f - (angle - settings.horizontalMaxProtectedAngle) / settings.horizontalFalloffRange;
        }

        // Quanto il tiratore e' sopraelevato rispetto al difensore: entro la
        // soglia della copertura la protezione resta piena, oltre decade
        // linearmente a 0 nel range di falloff.
        private static float ComputeVerticalFactor(Vector3 toShooter, CoverHeight coverHeight, CoverEffectivenessSettings settings)
        {
            Vector3 toShooterXZ = new Vector3(toShooter.x, 0f, toShooter.z);
            float horizontalDistance = toShooterXZ.magnitude;

            float elevationAngle = horizontalDistance > 0.0001f
                ? Mathf.Max(0f, Mathf.Atan2(toShooter.y, horizontalDistance) * Mathf.Rad2Deg)
                : 90f;

            float maxProtectedAngle = coverHeight == CoverHeight.High
                ? settings.highCoverMaxProtectedElevationAngle
                : settings.lowCoverMaxProtectedElevationAngle;
            float falloffRange = coverHeight == CoverHeight.High
                ? settings.highCoverElevationFalloffRange
                : settings.lowCoverElevationFalloffRange;

            if (elevationAngle <= maxProtectedAngle) return 1f;
            if (falloffRange <= 0f || elevationAngle >= maxProtectedAngle + falloffRange) return 0f;

            return 1f - (elevationAngle - maxProtectedAngle) / falloffRange;
        }
    }
}
