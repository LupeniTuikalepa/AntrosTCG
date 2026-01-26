using ATCG.Cards.Implementations;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;

namespace ATCG.Battle.Cards
{
    public class HeroBattleCard : BattleCard<HeroCardData>, IHeroCard
    {
        public int Health => Data.Health;
        public int DeathCost => Data.DeathCost;
        public int Speed => Data.Speed;
        public int Strength => Data.Strength;

        public HeroBattleCard(HeroCardData data, int playerID) : base(data, playerID)
        {

        }
    }
}