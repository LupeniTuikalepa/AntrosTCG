using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Players
{
    public interface IPlayerCommandListener<in T> : ICommandListener<T> where T : IPlayerCommand
    {
        IBattlePlayer BattlePlayer { get; }

        bool ICommandListener<T>.CanPlay(T command) => BattlePlayer != null && command.PlayerID == BattlePlayer.GetBattleID();
    }
}