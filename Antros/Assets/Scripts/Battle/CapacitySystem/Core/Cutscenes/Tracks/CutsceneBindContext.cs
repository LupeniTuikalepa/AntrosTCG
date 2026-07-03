// CutsceneBindContext.cs

using ATCG.Battle.Players.Local.Runtime;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    public readonly struct CutsceneBindContext
    {
        public readonly CastCapacityPhase phase;
        public readonly RuntimeLocalBattlePlayer screenPlayer;

        public CutsceneBindContext(CastCapacityPhase phase, RuntimeLocalBattlePlayer screenPlayer)
        {
            this.phase = phase;
            this.screenPlayer = screenPlayer;
        }
    }
}