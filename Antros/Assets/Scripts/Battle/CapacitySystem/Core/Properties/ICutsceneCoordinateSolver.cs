using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Properties
{
    /// <summary>
    /// Resolves hex coordinates to world positions for cutscene elements. In game it
    /// wraps the runtime battle grid; in the editor preview it uses a flat analytic
    /// conversion. Provided through the context (key COORDINATE_SOLVER) so elements
    /// place VFX from coordinates without depending on the runtime grid directly.
    /// </summary>
    public interface ICutsceneCoordinateSolver
    {
        Vector3 ToWorld(HexCoordinates coordinates);
    }
}