using System;

namespace ATCG.Capacities.Properties
{
    [Serializable]
    public sealed class IntPropertyDefinition : CapacityPropertyDefinition
    {
        public override Type ElementType => typeof(int);
    }
}