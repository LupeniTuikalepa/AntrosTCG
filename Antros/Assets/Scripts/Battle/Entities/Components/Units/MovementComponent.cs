using ATCG.Enums;
using Helteix.ChanneledProperties.Formulas;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct MovementComponent : IEntityComponent
    {
        public readonly Formula<float> moveSpeed;
        public readonly MovementType movementType;

        public int Speed => Mathf.FloorToInt(moveSpeed.Value);

        public MovementComponent(int baseMoveSpeed, MovementType movementType)
        {
            this.moveSpeed = new Formula<float>(baseMoveSpeed);
            this.movementType = movementType;
        }
    }
}
