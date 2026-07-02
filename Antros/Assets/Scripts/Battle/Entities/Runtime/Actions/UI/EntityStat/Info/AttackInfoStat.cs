using ATCG.Battle.Entities.Components;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class AttackInfoStat : HoverStateUIElement
	{
		[SerializeField] private GameObject attackBar;
		[SerializeField] private TMP_Text attackText;
		public override bool Build()
		{
			if (EntityPhase.HoveredAddress.TryGetComponentRO(out BasicAttackerComponent basicAttacker))
			{
				attackBar.SetActive(true);
				attackText.text = basicAttacker.Strength.ToString();
				return true;
			}
			attackBar.SetActive(false);
				return false;
		}
	}
}