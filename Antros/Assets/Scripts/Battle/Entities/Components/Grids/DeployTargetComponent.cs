using System.ComponentModel;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.Entities.Components
{
	public struct DeployTargetComponent : IEntityComponent
	{
		public readonly PatternGroup deployPattern;

		public DeployTargetComponent(PatternGroup deployPattern)
		{
			this.deployPattern = deployPattern;
		}
	}
}