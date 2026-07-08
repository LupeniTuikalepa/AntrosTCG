using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.Entities.Runtime;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Capacities.Devastation
{
    public class DevastationBallSetup : MonoBehaviour, ICapacityCutsceneElement
    {
        [SerializeField]
        private Transform start;
        [SerializeField]
        private Transform destination;
        [SerializeField]
        private ParticleSystem initialBall;
        [SerializeField]
        private ParticleSystem explosion;

        [SerializeField]
        private float ballHeight;
        [SerializeField]
        private float ballDistance;

        public void Connect(ICapacityContext context)
        {
            if (!context.TryGetProperty(CapacityContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;
            if (!context.TryGetProperty(CapacityContextKeys.CAST_POINT, out HexCoordinates castPoint))
                return;
            if (!context.TryGetProperty(CapacityContextKeys.COORDINATE_SOLVER, out ICutsceneCoordinateSolver solver))
                return;

            Vector3 hitPosition = solver.ToWorld(castPoint);

            Vector3 toPosition = (hitPosition - caster.transform.position).normalized;
            toPosition.y = 0;

            ballHeight = .3f;

            Vector3 startBallPosition = caster.transform.position + Vector3.up * ballHeight + toPosition * ballDistance;

            initialBall.transform.position = startBallPosition;
            start.position = startBallPosition;

            explosion.transform.position = hitPosition;
            destination.position = hitPosition;
        }

        public void Disconnect()
        {

        }
    }
}