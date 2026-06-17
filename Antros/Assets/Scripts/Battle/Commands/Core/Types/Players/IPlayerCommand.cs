using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Players
{
    public interface IPlayerCommand : ICommand
    {
        BattleID PlayerID { get;  }

        IBattlePlayer GetPlayer(BattlePhase battlePhase);
    }
}