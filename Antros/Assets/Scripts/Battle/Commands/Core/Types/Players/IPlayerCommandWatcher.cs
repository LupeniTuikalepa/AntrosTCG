using ATCG.Battle.Commands.Watchers;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Players
{
    public interface IPlayerCommandWatcher<in T> : ICommandWatcher<T> where T : IPlayerCommand
    {
        IBattlePlayer BattlePlayer { get; }

        bool ICommandWatcher<T>.Accepts(T command) => BattlePlayer != null && command.PlayerID == BattlePlayer.GetBattleID();
    }
}