using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class AttackInfoStat : InspectorInfoStatElement<BasicAttackerComponent>
	{
		protected override string GetText(BasicAttackerComponent component)
		{
			return component.Strength.ToString();
		}

		protected override float GetFillAmount(BasicAttackerComponent component)
		{
			return 1;
		}
	}
}