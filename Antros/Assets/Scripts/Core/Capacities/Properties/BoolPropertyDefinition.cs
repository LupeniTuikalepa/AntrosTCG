using System;

namespace ATCG.Capacities.Properties
{
    [Serializable]
    public sealed class BoolPropertyDefinition : CapacityPropertyDefinition
    {
        public override Type ElementType => typeof(bool);
    }
}