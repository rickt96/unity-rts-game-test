using UnityEngine;
using UnityEngine.InputSystem;
using Tactical.Units;

namespace Tactical.CommandUI
{
    // Selezione tra le squadre giocabili attive (Tab) e, dentro la squadra
    // corrente, del singolo membro (tasti 1-4): la camera si aggancia al
    // membro selezionato, e la squadra selezionata e' quella su cui
    // l'indicatore ad anello impartisce gli ordini.
    public class SquadSelector : MonoBehaviour
    {
        private int selectedSquadIndex;
        private int selectedMemberIndex;

        public Squad CurrentSquad => Squad.All.Count > 0
            ? Squad.All[Mathf.Clamp(selectedSquadIndex, 0, Squad.All.Count - 1)]
            : null;

        public UnitController CurrentMember
        {
            get
            {
                var squad = CurrentSquad;
                if (squad == null || squad.Members.Count == 0) return null;
                return squad.Members[Mathf.Clamp(selectedMemberIndex, 0, squad.Members.Count - 1)];
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.tabKey.wasPressedThisFrame)
                SelectNextSquad();

            if (keyboard.digit1Key.wasPressedThisFrame) SelectMember(0);
            else if (keyboard.digit2Key.wasPressedThisFrame) SelectMember(1);
            else if (keyboard.digit3Key.wasPressedThisFrame) SelectMember(2);
            else if (keyboard.digit4Key.wasPressedThisFrame) SelectMember(3);
        }

        public void SelectNextSquad()
        {
            if (Squad.All.Count == 0) return;
            selectedSquadIndex = (selectedSquadIndex + 1) % Squad.All.Count;
            selectedMemberIndex = 0;
        }

        public void SelectMember(int index)
        {
            var squad = CurrentSquad;
            if (squad == null || index < 0 || index >= squad.Members.Count) return;
            selectedMemberIndex = index;
        }
    }
}
