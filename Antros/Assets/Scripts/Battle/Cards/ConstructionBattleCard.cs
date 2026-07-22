using System;
using ATCG.Battle.Players;
using ATCG.Cards.Implementations;
using ATCG.Construction;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.Cards
{
    [Serializable]
    public class ConstructionBattleCard : BattleCard<ConstructionCardData>, IConstructionCard
    {
        public int MaxHealth => Data.Health;
        public int DeathCost => Data.DeathCost;
        public int Defense => Data.Defense;
        public PatternGroup DeployPatterns => Data.DeployPatterns;
        public ConstructionData ConstructionData => Data.ConstructionData;

        public ConstructionBattleCard(ConstructionCardData data, IBattlePlayer player) : base(data, player)
        {
        }
    }
}