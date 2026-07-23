using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Players
{
    public interface IPlayerCommandDirector<in T> : ICommandDirector<T> where T : IPlayerCommand
    {
        IBattlePlayer BattlePlayer { get; }

        bool ICommandDirector<T>.CanPlay(T command) => BattlePlayer != null && command.PlayerID == BattlePlayer.GetBattleID();
    }
}