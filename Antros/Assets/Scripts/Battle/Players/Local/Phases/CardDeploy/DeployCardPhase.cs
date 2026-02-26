using ATCG.Battle.Cards;
using ATCG.Battle.Grids;

namespace ATCG.Battle.Players.Local.Phases.CardDeploy
{
    public class DeployCardPhase : SelectCellPhase
    {
        public readonly IBattleCard card;

        public DeployCardPhase(LocalBattlePlayer player, BattleGrid battleGrid, IBattleCard card) : base(new DeployCardCellFilter(), battleGrid, player)
        {
            this.card = card;
        }
    }
}