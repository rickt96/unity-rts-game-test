using System.Collections.Generic;
using UnityEngine;

namespace Tactical.Units
{
    // Raggruppamento di soldati che si muovono e ricevono ordini insieme
    // (es. Alpha, Bravo). La soppressione da volume di fuoco e' calcolata
    // a questo livello, non per singolo soldato (vedi SquadSuppressionTracker).
    public class Squad : MonoBehaviour
    {
        [SerializeField] private string squadLabel = "Alpha";
        [SerializeField] private List<UnitController> members = new List<UnitController>();

        // Registro delle squadre attive: usato da SquadSelector per ciclare
        // (Tab) tra le squadre giocabili senza wiring manuale in scena.
        public static readonly List<Squad> All = new List<Squad>();

        public string SquadLabel => squadLabel;
        public IReadOnlyList<UnitController> Members => members;

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);
    }
}
