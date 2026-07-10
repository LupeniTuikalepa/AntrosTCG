using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using Unity.Mathematics;

namespace ATCG.Battle.Commands.GameCommands
{
    public class FillPlayerHandCommand : PlayerCommand<NoInfos>
    {
        public FillPlayerHandCommand(IBattlePlayer battlePlayer) : base(battlePlayer)
        {
        }

        protected override void Process(in CommandContext context)
        {
            if (GetPlayer(context.battlePhase) is LocalBattlePlayer localBattlePlayer)
            {
                localBattlePlayer.FillHand();
            }
        }
    }
}