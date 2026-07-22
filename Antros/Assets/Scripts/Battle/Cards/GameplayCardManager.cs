using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.Cards.Implementations;

namespace ATCG.Battle.Cards
{
    public static class GameplayCardManager
    {
        public static IBattleCard CreateCardFor(GameCardData data, IBattlePlayer player)
        {
            return data switch
            {
                HeroCardData heroCardData => new HeroBattleCard(heroCardData, player),
                ConstructionCardData constructionCardData => new ConstructionBattleCard(constructionCardData, player),
                _ => null
            };
        }
    }
}