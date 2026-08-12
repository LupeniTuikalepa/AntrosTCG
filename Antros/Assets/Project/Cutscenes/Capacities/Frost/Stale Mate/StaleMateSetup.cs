using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.HexGrids;
using UnityEngine;

using ATCG.Cutscenes;
namespace ATCG.Capacities.StaleMate
{
    public class StaleMateSetup : MonoBehaviour, ICapacityCutsceneElement
    {
	    [SerializeField] private Transform beamParent;

        public void Connect(ICutsceneContext context)
        {
	        if (!context.TryGetProperty(CutsceneContextKeys.CAST_POINT, out HexCoordinates castPoint))
		        return;
	        if (!context.TryGetProperty(CutsceneContextKeys.COORDINATE_SOLVER, out ICutsceneCoordinateSolver solver))
		        return;

	        Vector3 hitPosition = solver.ToWorld(castPoint);
	        beamParent.transform.position = hitPosition;
        }

        public void Disconnect()
        {
	        
        }
    }
}
