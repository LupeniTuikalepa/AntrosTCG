using System;
using ATCG.Battle.Players;
using ATCG.Capacities.Data;
using ATCG.Cards.Implementations;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
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
        public PatternGroup MovementPatterns => Data.MovementPatterns;
        public PatternGroup DeployementPatterns => Data.DeployPatterns;

        public HeroBattleCard(HeroCardData data, IBattlePlayer player) : base(data, player)
        {

        }
    }
}