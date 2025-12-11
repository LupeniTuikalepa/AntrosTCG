using ATCG.Cards.Implementations;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;

namespace ATCG.Battle.Cards
{
    public class HeroBattleCard : BattleCard<HeroCardData>
    {

        public HeroBattleCard(HeroCardData data, int playerID) : base(data, playerID)
        {

        }
    }
}