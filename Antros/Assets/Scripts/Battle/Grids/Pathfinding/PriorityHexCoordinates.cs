using System;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public readonly struct PriorityHexCoordinates : IComparable<PriorityHexCoordinates>
    {
        private readonly int priority;
        public readonly HexCoordinates coordinates;

        public PriorityHexCoordinates(HexCoordinates coordinates, int priority)
        {
            this.priority = priority;
            this.coordinates = coordinates;
        }

        public int CompareTo(PriorityHexCoordinates other)
        {
            return priority.CompareTo(other.priority);
        }
    }
}