using ATCG.Battle.CapacitySystem.Capacities.Frost;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using ATCG.Metrics;
using UnityEngine;

using ATCG.Cutscenes;
namespace ATCG.Capacities.Frost
{
    public class IceSpearSetup : MonoBehaviour, ICapacityCutsceneElement
    {
        [SerializeField]
        private Transform scalableSpearRoot;

        void ICutsceneElement.Connect(ICutsceneContext context)
        {
            if (context.TryGetProperty(IceSpear.HIT_DISTANCE_PROPERTY, out int distance))
            {
                float worldDistance = context.GetCoordinateSolver().ToWorldDistance(distance);
                Debug.Log(worldDistance);
                scalableSpearRoot.localScale = new Vector3(1, 1, worldDistance);
            }
         }

        void ICutsceneElement.Disconnect()
        {

        }
    }
}