using Helteix.ChanneledProperties.Formulas;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    public readonly struct BasicAttackerComponent : IEntityComponent
    {
        public readonly Formula<float> strength;

        public int Strength => Mathf.FloorToInt(strength.Value);

        public BasicAttackerComponent(int baseStrength)
        {
            this.strength = new Formula<float>(baseStrength);
        }
    }
}