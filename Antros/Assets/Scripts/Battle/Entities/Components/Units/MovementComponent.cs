using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Formulas;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct MovementComponent : IEntityComponent
    {
        public readonly Formula<float> moveSpeed;
        
        public readonly CapacityPatternData[] patternDatas;

        public int Speed => Mathf.FloorToInt(moveSpeed.Value);

        public MovementComponent(int baseMoveSpeed, CapacityPatternData[] patternDatas)
        {
            this.moveSpeed = new Formula<float>(baseMoveSpeed);
            this.patternDatas = patternDatas;
        }
    }
}