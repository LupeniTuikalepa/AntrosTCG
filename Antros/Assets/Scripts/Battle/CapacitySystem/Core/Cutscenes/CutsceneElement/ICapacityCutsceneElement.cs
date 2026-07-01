using ATCG.Battle.Players.Local.Runtime;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.CutsceneElement
{
    public interface ICapacityCutsceneElement
    {
        void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer, CastCapacityPhase capacityPhase);
    }
}