using ATCG.Battle.Entities.Components;
using ATCG.Capacities;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class SetupCapaPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text Description;
		[SerializeField] private TMP_Text ManaCost;

		public void SetupCapa(CapacityData capacityData)
		{
			Title.text = capacityData.Name;
			Description.text = capacityData.Description;
			ManaCost.text = capacityData.Cost.ToString();
		}
	}
}