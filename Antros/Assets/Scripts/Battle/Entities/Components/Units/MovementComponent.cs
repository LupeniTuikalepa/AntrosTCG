
using ATCG.Capacities.Data;
using ATCG.Enums;
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
        public readonly MovementType movementType;

        public int Speed => Mathf.FloorToInt(moveSpeed.Value);

        public MovementComponent(int baseMoveSpeed,  PatternGroup pattern, MovementType movementType)
        {
            this.moveSpeed = new Formula<float>(baseMoveSpeed);
            this.pattern = pattern;
            this.movementType = movementType;
        }
    }
}