using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Cards.Implementations;
using ATCG.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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