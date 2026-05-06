using Helteix.ChanneledProperties.Formulas;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct PhysicAttackerComponent : IEntityComponent
    {
        public readonly Formula<float> strength;

        public int Strength => Mathf.FloorToInt(strength.Value);

        public PhysicAttackerComponent(int baseStrength)
        {
            this.strength = new Formula<float>(baseStrength);
        }
    }
}