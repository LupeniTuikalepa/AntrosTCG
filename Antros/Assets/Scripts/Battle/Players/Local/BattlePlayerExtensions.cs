using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;

namespace ATCG.Battle.Players.Local
{
    public static class BattlePlayerExtensions
    {
        public static bool TryGetRuntime(this LocalBattlePlayer localBattlePlayer, out RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
            => RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor(localBattlePlayer, out runtimeLocalBattlePlayer);

        public static RuntimeLocalBattlePlayer GetRuntime(this LocalBattlePlayer localBattlePlayer) =>
            RuntimeLocalBattlePlayer.TryGetRuntimeLocalPlayerFor(localBattlePlayer, out var rlbp) ? rlbp : null;

    }
}