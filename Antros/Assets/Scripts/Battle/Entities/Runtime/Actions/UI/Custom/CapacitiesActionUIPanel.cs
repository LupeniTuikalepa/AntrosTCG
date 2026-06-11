using ATCG.Battle.Entities.Components;
using ATCG.Capacities;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI.Custom
{
    public class CapacitiesActionUIPanel : EntityActionUIPanel
    {
        [SerializeField]
        private Transform buttonContainer;

        [SerializeField]
        private CapacityButton buttonPrefab;


        public override bool Build()
        {
            buttonContainer.ClearChildren();

            foreach (var action in Phase.GetAll<CastCapacityAction>())
            {
                CapacityButton button = buttonPrefab.InstantiatePrefab(buttonContainer);
                CapacityData capacityData = action.capacityData;

                button.gameObject.name = capacityData.Name;
                button.SetCapacity(capacityData);

            }

            CollectButtons();
            return base.Build();
        }
    }
}