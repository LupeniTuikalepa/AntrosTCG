using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Listeners
{
    public interface IPlayerSignalListener : 
        IBaseSignalListener<PlayerCommandSignal>
    {
        IBattlePlayer BattlePlayer { get; }
        
        bool ICommandListener<PlayerCommandSignal>.Accepts(CommandContext context, PlayerCommandSignal command) => 
            HasSource(command) 
            && BattlePlayer != null && command.PlayerID == BattlePlayer.GetBattleID();
    }
}