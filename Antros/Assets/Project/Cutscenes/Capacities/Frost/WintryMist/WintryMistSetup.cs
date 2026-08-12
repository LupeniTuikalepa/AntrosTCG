using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.HexGrids;
using UnityEngine;

using ATCG.Cutscenes;
namespace ATCG.Capacities
{
    public class WintryMistSetup : MonoBehaviour, ICapacityCutsceneElement
    {
        [SerializeField]
        private Transform spreadParticles;


        public void Connect(ICutsceneContext context)
        {
            if (!context.TryGetProperty(CutsceneContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            if (!context.TryGetProperty("AimedCoord", out HexCoordinates aimedCoord))
                return;

            if (!context.TryGetProperty(CutsceneContextKeys.COORDINATE_SOLVER, out ICutsceneCoordinateSolver solver))
                return;

            Vector3 to = solver.ToWorld(aimedCoord);
            Vector3 from = caster.transform.position;

            spreadParticles.transform.forward = to - from;
        }

        public void Disconnect()
        {

        }
    }
}