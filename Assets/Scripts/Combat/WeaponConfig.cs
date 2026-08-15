using UnityEngine;

namespace Tactical.Combat
{
    // Un archetipo d'arma. Per la demo: Leggera e Pesante. Granate e altre
    // armi speciali sono fuori scope e verranno modellate a parte.
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Tactical/Weapon Config")]
    public class WeaponConfig : ScriptableObject
    {
        public string weaponName;

        [Tooltip("Colpi al secondo")]
        public float fireRate = 2f;

        [Tooltip("Peso di questa arma nell'accumulo di esposizione del bersaglio colpito")]
        public float exposureWeight = 1f;

        [Tooltip("Peso di questa arma nell'accumulo di soppressione della squadra bersaglio")]
        public float suppressionWeight = 1f;

        [Tooltip("Tempo (secondi) prima che il riflesso di sopravvivenza possa attivarsi sotto il fuoco di quest'arma")]
        public float survivalReflexGracePeriod = 1f;
    }
}
