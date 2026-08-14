using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Cutscenes
{
    public class SetUpHuntOfTheSoul : MonoBehaviour, ICapacityCutsceneElement
    {
        [SerializeField] private Transform to;
        
        public void Connect(ICutsceneContext context)
        {
            if (!context.TryGetProperty(CutsceneContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;
            if (!context.TryGetProperty(CutsceneContextKeys.CAST_POINT, out HexCoordinates castPoint))
                return;
            if (!context.TryGetProperty(CutsceneContextKeys.COORDINATE_SOLVER, out ICutsceneCoordinateSolver solver))
                return;
            
            to.position = solver.ToWorld(castPoint);
        }

        public void Disconnect()
        {
        }
    }
}
