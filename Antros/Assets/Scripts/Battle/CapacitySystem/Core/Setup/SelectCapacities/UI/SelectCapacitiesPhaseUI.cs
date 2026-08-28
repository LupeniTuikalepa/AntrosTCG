using ATCG.Battle.CapacitySystem.Core.Setup.CopyCapa.UI;
using ATCG.Battle.CapacitySystem.Core.Setup.SelectCapacities;
using ATCG.Battle.Entities.Runtime.UI;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using ATCG.Utilities;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATCG.Battle.CapacitySystem.Core.Setup.CopyCapa
{
	public class SelectCapacitiesPhaseUI : LocalPlayerMonoPhaseListener<SelectCapacitiesPhase>
	{
		[SerializeField] private SelectCapacityUIItem capaUIPrefab;
		[SerializeField] private Transform container;
		[SerializeField] private CanvasGroup canvasGroup;

		private SelectCapacitiesPhase current;
       
		private void Start()
		{
			canvasGroup.Hide(0f);
			container.ClearChildren();
		}

		protected override void OnPhaseBegin(SelectCapacitiesPhase phase)
		{
			base.OnPhaseBegin(phase);
			current =  phase;

			container.ClearChildren();
			canvasGroup.Show(.2f);
          
			foreach (CapacityData capacityData in phase.capacities)
			{
				var clone = Instantiate(capaUIPrefab, container);
				clone.Initialize(phase, capacityData);
			}
		}

		protected override void OnPhaseEnd(SelectCapacitiesPhase phase)
		{
			base.OnPhaseEnd(phase);
			canvasGroup.Hide(.2f);
			container.ClearChildren();
		}

		public void Close() => current?.SetResult(null);
	}
}