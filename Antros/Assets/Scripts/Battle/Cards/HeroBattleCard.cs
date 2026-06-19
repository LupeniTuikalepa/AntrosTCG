using System;
using ATCG.Battle.Players;
using ATCG.Capacities.Data;
using ATCG.Cards.Implementations;
using Helteix.ChanneledProperties.Formulas;

namespace ATCG.Battle.Cards
{
    [Serializable]
    public class HeroBattleCard : BattleCard<HeroCardData>, IHeroCard
    {
        public int DeathCost => Data.DeathCost;
        public int MaxHealth => Data.Health;
        public int Speed => Data.Speed;
        public int Strength => Data.Strength;
        public PatternData[] MovementPatterns => Data.MovementPatterns.Data;

        public HeroBattleCard(HeroCardData data, IBattlePlayer player) : base(data, player)
        {

        }
    }
}