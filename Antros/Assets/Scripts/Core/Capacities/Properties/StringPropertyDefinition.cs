using System;

namespace ATCG.Capacities.Properties
{
    [Serializable]
    public sealed class StringPropertyDefinition : CapacityPropertyDefinition
    {
        public override Type ElementType => typeof(string);
    }
}