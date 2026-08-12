using ATCG.Capacities;
using ATCG.Capacities.UI;
using ATCG.Cards.Implementations;
using CopyCapa;
using UnityEngine;

namespace StealCapa.UI
{
	public class CapaSelected : MonoBehaviour
	{
		[field: SerializeField] public CapacityUI SelectedCapa { get; private set; }
		

		private CopyCapaPhase phase;

		private void Awake()
		{
			if (SelectedCapa == null)
				SelectedCapa = GetComponent<CapacityUI>();
		}

		public void Initialize(CopyCapaPhase phase)
		{
			this.phase = phase;
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
		
		
		
	}
}