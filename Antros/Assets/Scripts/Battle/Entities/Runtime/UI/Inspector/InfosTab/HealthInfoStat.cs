using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class HealthInfoStat : InspectorInfoStatElement<HealthComponent>
	{
		protected override string GetText(HealthComponent component)
		{
			return $"{component.CurrentHealth}/{component.MaxHealth}";
		}

		protected override float GetFillAmount(HealthComponent component)
		{
			return (float)component.CurrentHealth / component.MaxHealth;
		}
	}
}