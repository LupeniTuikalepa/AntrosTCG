
using ATCG.Capacities.Data;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Formulas;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct MovementComponent : IEntityComponent
    {
        public readonly Formula<float> moveSpeed;

        public readonly PatternGroup pattern;

        public int Speed => Mathf.FloorToInt(moveSpeed.Value);

        public MovementComponent(int baseMoveSpeed,  PatternGroup pattern)
        {
            this.moveSpeed = new Formula<float>(baseMoveSpeed);
            this.pattern = pattern;
        }
    }
}