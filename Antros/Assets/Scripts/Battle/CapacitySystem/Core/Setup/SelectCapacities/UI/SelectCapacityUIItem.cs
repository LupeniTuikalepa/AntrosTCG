using ATCG.Battle.CapacitySystem.Core.Setup.SelectCapacities;
using ATCG.Capacities;
using ATCG.Capacities.UI;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Setup.CopyCapa.UI
{
	public class SelectCapacityUIItem : MonoBehaviour
	{
		[field: SerializeField]
		public CapacityUI SelectedCapa { get; private set; }
		private SelectCapacitiesPhase phase;

		private void Awake()
		{
			if (SelectedCapa == null)
				SelectedCapa = GetComponent<CapacityUI>();
		}

		public void Click()
		{
			CapacityData selectedData = SelectedCapa.Current;

			if (selectedData != null && phase != null)
			{
				Debug.Log($"[StealCapa] Capacité sélectionnée : {selectedData.Name}");
				phase.SetResult(selectedData);
			}
		}

		public void Initialize(SelectCapacitiesPhase selectCapacitiesPhase, CapacityData capacityData)
		{
			phase = selectCapacitiesPhase;
			SelectedCapa.Connect(capacityData);
		}
	}
}