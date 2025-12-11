using ATCG.Cards;
using ATCG.Cards.Implementations;

namespace ATCG.Battle.Cards
{
    public static class GameplayCardManager
    {
        public static IBattleCard CreateCardFor(GameCardData data, int player) => data switch
        {
            HeroCardData heroCardData => new HeroBattleCard(heroCardData, player),
            _ => null
        };
    }
}