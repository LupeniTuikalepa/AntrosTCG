using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.CutsceneElement;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;

namespace Project.Capacities.Devastation
{
    public class DevastationHeroSetup : MonoBehaviour, ICapacityCutsceneElement
    {
        public void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer, CastCapacityPhase capacityPhase)
        {
            if (capacityPhase.TryGetRuntimeCaster(runtimeLocalBattlePlayer, out IRuntimeEntity runtimeEntity))
            {
                runtimeEntity.LookAtCoord(capacityPhase.castPoint, .15f);
            }
        }
    }
}