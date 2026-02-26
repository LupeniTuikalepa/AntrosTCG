using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.Cards.Implementations;

namespace ATCG.Battle.Cards
{
    public static class GameplayCardManager
    {
        public static IBattleCard CreateCardFor(GameCardData data, IBattlePlayer player) => data switch
        {
            HeroCardData heroCardData => new HeroBattleCard(heroCardData, player),
            _ => null
        };
    }
}