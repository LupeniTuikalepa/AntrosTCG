using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands
{
    public interface IPlayerCommandListener<in T> : ICommandListener<T> where T : IPlayerCommand
    {
        IBattlePlayer BattlePlayer { get; }

        bool ICommandListener<T>.CanPlay(T command) => BattlePlayer != null && command.PlayerID == BattlePlayer.GetBattleID();
    }
}