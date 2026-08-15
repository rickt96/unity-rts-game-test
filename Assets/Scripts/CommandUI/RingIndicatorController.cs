using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Tactical.Units;
using Tactical.Cover;
using Tactical.Orders;

namespace Tactical.CommandUI
{
    // Indicatore ad anello che segue il cursore sul terreno: se il punto e'
    // vicino ad un CoverPoint mostra un anello per OGNI posizione che la
    // squadra andrebbe ad occupare (uno slot proprio o in stack, riusando
    // CoverSlotAssigner), altrimenti mostra un anello per ogni soldato
    // nella sua posizione di formazione (FormationUtility). Click sinistro
    // conferma l'ordine sulla squadra correntemente selezionata
    // (SquadSelector, tasto Tab). Se la copertura non ha capacita'
    // sufficiente per l'intero gruppo, un singolo anello rosso segnala il
    // rifiuto invece dell'anteprima per-soldato.
    public class RingIndicatorController : MonoBehaviour
    {
        [SerializeField] private Camera cameraToUse;
        [SerializeField] private SquadSelector squadSelector;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float snapRadius = 2f;
        [SerializeField] private float stackPreviewOffset = 0.6f;
        [SerializeField] private Color freeColor = Color.white;
        [SerializeField] private Color snapColor = Color.green;
        [SerializeField] private Color stackColor = new Color(1f, 0.6f, 0f);
        [SerializeField] private Color rejectedColor = Color.red;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private const int RingSegments = 32;
        private const float RingRadius = 0.6f;

        private readonly List<LineRenderer> ringPool = new List<LineRenderer>();
        private readonly List<Vector3> drawPositions = new List<Vector3>();
        private readonly List<Color> drawColors = new List<Color>();
        private Material sharedRingMaterial;
        private MaterialPropertyBlock propertyBlock;

        private bool hasValidTarget;
        private Vector3 targetPoint;
        private CoverPoint snappedCoverPoint;
        private List<CoverSlotAssigner.Assignment> previewAssignments;
        private Vector3[] formationPreview;

        private void Awake()
        {
            if (cameraToUse == null) cameraToUse = Camera.main;

            sharedRingMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            UpdateTargetFromCursor();
            UpdateRingVisual();

            var mouse = Mouse.current;
            if (hasValidTarget && mouse != null && mouse.leftButton.wasPressedThisFrame)
                ConfirmOrder();
        }

        private void UpdateTargetFromCursor()
        {
            hasValidTarget = false;
            snappedCoverPoint = null;
            previewAssignments = null;
            formationPreview = null;

            var mouse = Mouse.current;
            if (cameraToUse == null || mouse == null) return;

            Ray ray = cameraToUse.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f, groundMask)) return;

            targetPoint = hit.point;
            hasValidTarget = true;

            CoverPoint nearest = FindNearestCoverPoint(targetPoint, snapRadius);
            if (nearest != null)
            {
                snappedCoverPoint = nearest;
                targetPoint = nearest.Slots[0].Position;
                previewAssignments = CoverSlotAssigner.TryAssign(nearest, GetControlledUnitControllers());
            }
            else
            {
                formationPreview = ComputeFormationDestinations(GetControlledUnitControllers(), targetPoint);
            }
        }

        private IReadOnlyList<UnitController> GetControlledUnitControllers()
        {
            var squad = squadSelector != null ? squadSelector.CurrentSquad : null;
            return squad != null ? squad.Members : (IReadOnlyList<UnitController>)System.Array.Empty<UnitController>();
        }

        private static CoverPoint FindNearestCoverPoint(Vector3 point, float radius)
        {
            CoverPoint best = null;
            float bestDistance = radius;

            foreach (var coverPoint in CoverPoint.All)
            {
                if (coverPoint.Slots.Count == 0) continue;

                float distance = Vector3.Distance(point, coverPoint.Slots[0].Position);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = coverPoint;
                }
            }

            return best;
        }

        // Stessa forma a cuneo usata da OrderController al momento
        // dell'ordine reale: indicizzata per posizione nell'elenco unita',
        // cosi' l'anteprima corrisponde esattamente a cio' che succedera'.
        private static Vector3[] ComputeFormationDestinations(IReadOnlyList<UnitController> units, Vector3 target)
        {
            var destinations = new Vector3[units.Count];
            if (units.Count == 0) return destinations;

            Vector3 centroid = Vector3.zero;
            int count = 0;
            foreach (var unit in units)
            {
                if (unit == null) continue;
                centroid += unit.transform.position;
                count++;
            }
            if (count == 0) return destinations;
            centroid /= count;

            Vector3 forward = target - centroid;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.01f ? forward.normalized : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            for (int i = 0; i < units.Count; i++)
            {
                Vector2 offset = FormationUtility.GetOffset(i, units.Count);
                destinations[i] = target + right * offset.x - forward * offset.y;
            }

            return destinations;
        }

        private void UpdateRingVisual()
        {
            drawPositions.Clear();
            drawColors.Clear();

            if (hasValidTarget)
                CollectPreviewMarkers();

            for (int i = 0; i < drawPositions.Count; i++)
            {
                LineRenderer ring = i < ringPool.Count ? ringPool[i] : CreateRing();
                ring.enabled = true;
                SetRingGeometry(ring, drawPositions[i]);
                propertyBlock.SetColor(BaseColorId, drawColors[i]);
                ring.SetPropertyBlock(propertyBlock);
            }

            for (int i = drawPositions.Count; i < ringPool.Count; i++)
                ringPool[i].enabled = false;
        }

        private void CollectPreviewMarkers()
        {
            if (snappedCoverPoint != null)
            {
                if (previewAssignments != null)
                {
                    foreach (var assignment in previewAssignments)
                    {
                        Vector3 position = assignment.Slot.Position;
                        if (assignment.IsStacked)
                            position -= assignment.Slot.Normal * stackPreviewOffset;

                        drawPositions.Add(position);
                        drawColors.Add(assignment.IsStacked ? stackColor : snapColor);
                    }
                }
                else
                {
                    drawPositions.Add(targetPoint);
                    drawColors.Add(rejectedColor);
                }
            }
            else if (formationPreview != null && formationPreview.Length > 0)
            {
                foreach (var position in formationPreview)
                {
                    drawPositions.Add(position);
                    drawColors.Add(freeColor);
                }
            }
            else
            {
                drawPositions.Add(targetPoint);
                drawColors.Add(freeColor);
            }
        }

        private LineRenderer CreateRing()
        {
            var ringObject = new GameObject("RingMarker");
            ringObject.transform.SetParent(transform, false);

            var lineRenderer = ringObject.AddComponent<LineRenderer>();
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = RingSegments;
            lineRenderer.widthMultiplier = 0.08f;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.material = sharedRingMaterial;

            ringPool.Add(lineRenderer);
            return lineRenderer;
        }

        private static void SetRingGeometry(LineRenderer lineRenderer, Vector3 center)
        {
            for (int i = 0; i < RingSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / RingSegments;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * RingRadius;
                lineRenderer.SetPosition(i, center + offset + Vector3.up * 0.05f);
            }
        }

        private void ConfirmOrder()
        {
            if (snappedCoverPoint != null)
            {
                if (previewAssignments == null) return; // capacita' insufficiente: ordine rifiutato

                foreach (var assignment in previewAssignments)
                {
                    if (assignment.Unit != null && assignment.Unit.TryGetComponent(out OrderController order))
                        order.IssueTakeCoverAssignment(assignment.Slot, assignment.IsStacked);
                }
            }
            else
            {
                IssueFormationMoveTo(GetControlledUnitControllers(), targetPoint);
            }
        }

        private static void IssueFormationMoveTo(IReadOnlyList<UnitController> units, Vector3 target)
        {
            if (units.Count == 0) return;

            Vector3[] destinations = ComputeFormationDestinations(units, target);
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null && units[i].TryGetComponent(out OrderController order))
                    order.IssueMoveTo(destinations[i]);
            }
        }
    }
}
