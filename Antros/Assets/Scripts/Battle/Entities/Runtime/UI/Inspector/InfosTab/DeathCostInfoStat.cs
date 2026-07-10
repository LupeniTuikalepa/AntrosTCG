using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class DeathCostInfoStat : InspectorInfoStatElement<DeathCostComponent>
	{
		protected override string GetText(DeathCostComponent component)
		{
			return component.cost.ToString();
		}

		protected override float GetFillAmount(DeathCostComponent component)
		{
			return 1;
		}
	}
}