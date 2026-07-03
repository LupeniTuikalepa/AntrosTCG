using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.CutsceneElement;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids.Runtime;
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

        public void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer, CastCapacityPhase capacityPhase)
        {
            if (capacityPhase.TryGetRuntimeCaster(runtimeLocalBattlePlayer, out IRuntimeEntity runtimeEntity))
            {
                RuntimeHexGrid runtimeHexGrid = runtimeLocalBattlePlayer.RuntimeBattleGrid.RuntimeHexGrid;
                Vector3 hitPosition = runtimeHexGrid.GetPositionAt(capacityPhase.castPoint);

                Vector3 toPosition = (hitPosition - runtimeEntity.transform.position).normalized;
                toPosition.y = 0;

                ballHeight = .3f;

                Vector3 startBallPosition = runtimeEntity.transform.position + Vector3.up * ballHeight + toPosition * ballDistance;

                initialBall.transform.position = startBallPosition;
                start.position = startBallPosition;

                explosion.transform.position = hitPosition;
                destination.position = hitPosition;
            }
        }
    }
}