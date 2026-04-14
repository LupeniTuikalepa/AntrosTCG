using ATCG.Battle.Cards;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Formulas;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct HeroComponent : IEntityComponent
    {
        public readonly HeroBattleCard heroCard;

        public Formula<int> DeathCost => heroCard.DeathCost;
        public Formula<int> MaxHealth => heroCard.MaxHealth;
        public Formula<int> Speed => heroCard.Speed;
        public Formula<int> Strength => heroCard.Strength;

        public HeroComponent(HeroBattleCard heroBattleCard)
        {
            heroCard = heroBattleCard;
        }
    }
}