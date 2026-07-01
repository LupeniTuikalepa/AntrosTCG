using ATCG.Battle.Entities.Components;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class VelocityInfoStat : HoverStateUIElement
	{
		[SerializeField] private GameObject velocityBar;
		[SerializeField] private TMP_Text velocityText;
		public override bool Build()
		{
			if (EntityPhase.HoveredAddress.TryGetComponentRO(out MovementComponent movement))
			{
				velocityBar.SetActive(true);
				velocityText.text=movement.Speed.ToString();
				return true;
			}
			velocityBar.SetActive(false);
			return false;
		}
	}
}