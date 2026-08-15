using UnityEngine;

namespace Tactical.Orders
{
    // Offset di formazione (cuneo) per gli ordini Move To di gruppo: la
    // squadra non deve convergere tutta sullo stesso punto. Offset in
    // spazio locale (destra, indietro) rispetto alla direzione di marcia.
    public static class FormationUtility
    {
        private static readonly Vector2[] WedgeOffsets =
        {
            new Vector2(0f, 0f),
            new Vector2(1.2f, 1.2f),
            new Vector2(-1.2f, 1.2f),
            new Vector2(0f, 2.4f),
        };

        public static Vector2 GetOffset(int index, int count)
        {
            if (index < WedgeOffsets.Length) return WedgeOffsets[index];

            // Oltre i 4 posti previsti (dimensione massima di squadra nella
            // demo): ulteriore fila dietro, stessa logica a zig-zag.
            int row = index / 2;
            float side = (index % 2 == 0) ? 1f : -1f;
            return new Vector2(side * 1.2f, row * 1.2f);
        }
    }
}
