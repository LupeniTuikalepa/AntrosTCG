using System;
using ATCG.Battle.Players;
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


        public HeroBattleCard(HeroCardData data, IBattlePlayer player) : base(data, player)
        {

        }
    }
}