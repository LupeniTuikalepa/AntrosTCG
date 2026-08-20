using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Listeners
{
    public interface IPlayerSignalDirector : 
        IBaseSignalDirector<PlayerCommandSignal>
    {
        IBattlePlayer BattlePlayer { get; }

        bool ICommandDirector<PlayerCommandSignal>.CanPlay(PlayerCommandSignal command)
        {
            return HasSource(command) 
                && BattlePlayer != null && command.PlayerID == BattlePlayer.GetBattleID();
        }
    }
}