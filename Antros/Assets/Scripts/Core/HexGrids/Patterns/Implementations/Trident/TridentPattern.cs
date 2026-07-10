using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
	public readonly struct TridentPattern : IHexPattern
	{
		private readonly HexCoordinates target;
		private readonly TridentPatternData data;

		public TridentPattern(HexCoordinates target, TridentPatternData data)
		{
			this.target = target;
			this.data = data;
		}

		public IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller)
		{
			int centerIndex = FindDirectionIndex(from.GetDirection(target).NearestCardinal());
			if (centerIndex == -1)
				yield break;

			int halfSpread = (data.BranchCount - 1) / 2; // ex: BranchCount=5 -> halfSpread=2 -> offsets -2,-1,0,1,2
			int step = data.AngleStepInHexDirections;

			for (int branch = -halfSpread; branch <= halfSpread; branch++)
			{
				int idx = Mod(centerIndex + branch * step, HexOperations.DirectionsCount);
				HexCoordinates branchDirection = HexOperations.GetDirection((HexDirection)idx);
				HexCoordinates branchEnd = from + branchDirection.Multiply(data.Range);

				foreach (HexCoordinates coordinate in from.GetLine(branchEnd))
					yield return coordinate;
			}
		}

		private static int Mod(int value, int modulo) => ((value % modulo) + modulo) % modulo;

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