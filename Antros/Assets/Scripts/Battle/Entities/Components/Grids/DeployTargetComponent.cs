namespace ATCG.Battle.Entities.Components
{
	public struct DeployTargetComponent : IEntityComponent
	{
		public readonly int deployRange;

		public DeployTargetComponent(int deployRange)
		{
			this.deployRange = deployRange;
		}
	}
}