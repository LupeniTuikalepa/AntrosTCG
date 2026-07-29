using System;
using ATCG.Battle.Players;
using ATCG.Capacities.Data;
using ATCG.Cards.Implementations;
using ATCG.Enums;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Formulas;

namespace ATCG.Battle.Cards
{
    [Serializable]
    public class HeroBattleCard : BattleCard<HeroCardData>, IHeroCard
    {
        public int DeathCost => GameMetrics.Current.CardRarityDeathCost[Data.Rarity];

        public int MaxHealth => Data.Health;
        public int Strength => Data.Strength;
        public int Defense => Data.Defense;
        public int DeployRange => Data.DeployRange;

        public int Speed => Data.Speed;
        public MovementType MovementType => Data.MovementType;

        public HeroBattleCard(HeroCardData data, IBattlePlayer player) : base(data, player)
        {

        }
    }
}