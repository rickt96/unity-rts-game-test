using UnityEngine;

namespace Tactical.Combat
{
    // Un tiratore attivo su un bersaglio: posizione (per il calcolo
    // dell'angolo di copertura) e archetipo d'arma (per i pesi).
    public readonly struct ThreatSource
    {
        public readonly Transform Attacker;
        public readonly WeaponConfig Weapon;

        public ThreatSource(Transform attacker, WeaponConfig weapon)
        {
            Attacker = attacker;
            Weapon = weapon;
        }
    }
}
