using ATCG.HexGrids;
using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Editor-preview coordinate solver: a flat analytic hex->world conversion using
    /// GameMetrics.Current.CellRadius. Approximate (assumes pointy-top axial layout);
    /// good enough to preview VFX placement. If it visibly diverges from the runtime
    /// grid, mirror RuntimeHexGrid.GetPositionAt here instead.
    /// </summary>
    public sealed class PreviewCoordinateSolver : ATCG.Battle.CapacitySystem.Core.Properties.ICutsceneCoordinateSolver
    {
        public Vector3 ToWorld(HexCoordinates coordinates)
        {
            float radius = GameMetrics.Current != null ? GameMetrics.Current.CellRadius : 1f;

            // Pointy-top axial -> world (x = q, z = r). q/r read from the coordinate's
            // axial components. Adjust if your HexCoordinates exposes different fields.
            int q = coordinates.X;
            int r = coordinates.Y;

            float x = radius * Mathf.Sqrt(3f) * (q + r / 2f);
            float z = radius * 1.5f * r;

            return new Vector3(x, 0f, z);
        }
    }
}