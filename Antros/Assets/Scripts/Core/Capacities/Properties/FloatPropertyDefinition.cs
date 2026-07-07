using System;

namespace ATCG.Capacities.Properties
{
    [Serializable]
    public sealed class FloatPropertyDefinition : CapacityPropertyDefinition
    {
        public override Type ElementType => typeof(float);
    }
}