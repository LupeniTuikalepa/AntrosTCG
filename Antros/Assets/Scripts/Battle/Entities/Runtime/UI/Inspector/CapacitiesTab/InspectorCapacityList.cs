using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector.CapacitiesTab
{
    public class InspectorCapacityList : EntityInspectorTabElement
    {
        [SerializeField]
        private InspectorCapacityElement capacityElementPrefab;

        [SerializeField]
        private Transform container;

        private void Awake()
        {
            container.ClearChildren();
        }

        public override bool Connect(InspectEntityPhase phase)
        {
            if (phase.EntityAddress.TryGetComponentRO(out CapacityCasterComponent capacityCasterComponent))
            {
	            foreach (var capacity in capacityCasterComponent.capacities)
	            {
		            InspectorCapacityElement instance = capacityElementPrefab.InstantiatePrefab(container);
		            instance.Connect(capacity);
	            }

	            return true;
            }

            return false;
        }

        public override void Disconnect(InspectEntityPhase phase)
        {
            container.ClearChildren();
        }
    }
}