using UnityEngine;
using Unity.AI.Navigation;

namespace Tactical.Orders
{
    // Genera la NavMesh a runtime all'avvio della scena: il pacchetto AI
    // Navigation lo consente anche fuori editor, cosi' non serve fare Bake
    // manualmente ogni volta che il livello (o le coperture) cambia.
    public class NavMeshBootstrapper : MonoBehaviour
    {
        [SerializeField] private NavMeshSurface surface;

        private void Awake()
        {
            if (surface != null)
                surface.BuildNavMesh();
        }
    }
}
