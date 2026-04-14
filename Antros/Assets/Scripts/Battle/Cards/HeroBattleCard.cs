using ATCG.Battle.Players;
using ATCG.Cards.Implementations;
using Helteix.ChanneledProperties.Formulas;

namespace ATCG.Battle.Cards
{
    public class HeroBattleCard : BattleCard<HeroCardData>, IHeroCard
    {
        int IHeroCard.DeathCost => DeathCost;
        int IHeroCard.MaxHealth => MaxHealth;
        int IHeroCard.Speed => Speed;
        int IHeroCard.Strength => Strength;

        public Formula<int> DeathCost { get; private set; }
        public Formula<int> MaxHealth { get; private set; }
        public Formula<int> Speed { get; private set; }
        public Formula<int> Strength { get; private set; }


        public HeroBattleCard(HeroCardData data, IBattlePlayer player) : base(data, player)
        {
            MaxHealth = new Formula<int>(data.Health);
            Speed = new Formula<int>(data.Speed);
            Strength = new Formula<int>(data.Strength);
            DeathCost = new Formula<int>(data.DeathCost);
        }
    }
}