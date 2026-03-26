using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Core.Components;
using ATCG.Cards.Implementations;

namespace ATCG.Battle.Entities.Components
{
    public struct HeroComponent : IEntityComponent
    {
        public HeroBattleCard HeroBattleCard { get; }
        
        public HeroComponent(HeroBattleCard heroBattleCard)
        {
            HeroBattleCard = heroBattleCard;
        }
    }
}