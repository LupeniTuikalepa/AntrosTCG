using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
	public readonly struct TridentPattern : IHexPattern
	{
		private readonly HexCoordinates target;
		private readonly int range;

		public TridentPattern(HexCoordinates target, int range)
		{
			this.target = target;
			this.range = range;
		}

		public IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller)
		{
			int dirIndex = FindDirectionIndex(from.GetDirection(target).NearestCardinal());
			if (dirIndex == -1)
				yield break;

			for (int offset = -1; offset <= 1; offset++)
			{
				int idx = ((dirIndex + offset) % HexOperations.DirectionsCount + HexOperations.DirectionsCount) % HexOperations.DirectionsCount;
				HexCoordinates branchDirection = HexOperations.GetDirection((HexDirection)idx);
				HexCoordinates branchEnd = from + branchDirection.Multiply(range);

				foreach (HexCoordinates coordinate in from.GetLine(branchEnd))
					yield return coordinate;
			}
		}

		private static int FindDirectionIndex(HexCoordinates direction)
		{
			var directions = HexOperations.Directions;
			for (int i = 0; i < directions.Length; i++)
			{
				if (directions[i] == direction)
					return i;
			}
			return -1;
		}
	}
}