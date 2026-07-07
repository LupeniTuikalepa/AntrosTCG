using System;

namespace ATCG.Capacities.Properties
{
    [Serializable]
    public sealed class HexCoordinatesPropertyDefinition : CapacityPropertyDefinition
    {
        public override Type ElementType => typeof(ATCG.HexGrids.HexCoordinates);
    }
}