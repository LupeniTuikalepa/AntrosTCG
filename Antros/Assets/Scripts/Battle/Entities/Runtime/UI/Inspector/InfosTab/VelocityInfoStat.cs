using ATCG.Battle.Entities.Components;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class VelocityInfoStat : InspectorInfoStatElement<MovementComponent>
	{
		protected override string GetText(MovementComponent component)
		{
			return component.Speed.ToString();
		}

		protected override float GetFillAmount(MovementComponent component)
		{
			return 1;
		}
	}
}