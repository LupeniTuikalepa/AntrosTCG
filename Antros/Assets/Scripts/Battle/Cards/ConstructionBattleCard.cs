using System;
using ATCG.Battle.Players;
using ATCG.Cards.Implementations;
using ATCG.HexGrids.Patterns.Building;
using UnityEngine;

namespace ATCG.Battle.Cards
{
    [Serializable]
    public class ConstructionBattleCard : BattleCard<ConstructionCardData>, IConstructionCard
    {
        public int MaxHealth => Data.Health;
        public int DeathCost => Data.DeathCost;
        public int Defense => Data.Defense;
        public int DeployRange => Data.DeployRange;
        public GameObject Prefab => Data.Prefab;
        public int PassiveRange => Data.PassiveRange;

        public ConstructionBattleCard(ConstructionCardData data, IBattlePlayer player) : base(data, player)
        {
        }
    }
}