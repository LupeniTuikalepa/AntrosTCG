using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle.Cards;
using ATCG.Battle.HexGrids;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players
{
    public class DeployCardPhase : PhaseCompletionSource<BattleCell>
    {
        public readonly IBattleCard card;

        public DeployCardPhase(IBattleCard card)
        {
            this.card = card;
        }
    }
}