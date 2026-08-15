using System.Collections.Generic;
using UnityEngine;
using Tactical.Combat;

namespace Tactical.DebugTools
{
    // Disegna una linea da ogni attaccante attivo verso il membro di
    // squadra che sta attualmente colpendo (SquadSuppressionTracker.
    // CurrentTargets), per vedere a colpo d'occhio la concentrazione del
    // fuoco e come si sposta quando qualcuno si copre.
    public class TargetingDebugVisualizer : MonoBehaviour
    {
        [SerializeField] private Color lineColor = Color.red;
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private float heightOffset = 1.5f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly List<LineRenderer> linePool = new List<LineRenderer>();
        private Material lineMaterial;

        private void Awake()
        {
            lineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            lineMaterial.SetColor(BaseColorId, lineColor);
        }

        private void Update()
        {
            int used = 0;

            foreach (var tracker in SquadSuppressionTracker.All)
            {
                foreach (var pair in tracker.CurrentTargets)
                {
                    Transform attacker = pair.Key;
                    var target = pair.Value;
                    if (attacker == null || target == null) continue;

                    LineRenderer line = used < linePool.Count ? linePool[used] : CreateLine();
                    line.enabled = true;
                    line.SetPosition(0, attacker.position + Vector3.up * heightOffset);
                    line.SetPosition(1, target.transform.position + Vector3.up * heightOffset);
                    used++;
                }
            }

            for (int i = used; i < linePool.Count; i++)
                linePool[i].enabled = false;
        }

        private LineRenderer CreateLine()
        {
            var lineObject = new GameObject("TargetingLine");
            lineObject.transform.SetParent(transform, false);

            var lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.widthMultiplier = lineWidth;
            lineRenderer.material = lineMaterial;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            linePool.Add(lineRenderer);
            return lineRenderer;
        }
    }
}
