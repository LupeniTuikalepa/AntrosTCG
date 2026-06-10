using ATCG.Battle.Commands.GameCommands;
using ATCG.Capacities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.UI.Custom
{
    public class CapacityButton : EntityActionUIActionButton
    {
        [SerializeField]
        private TMP_Text capacityName;
        [SerializeField]
        private TMP_Text description;

        private CapacityData capacityData;


        public void SetCapacity(CapacityData newCapacityData)
        {
            capacityData = newCapacityData;
        }

        protected override int GetCost() => capacityData.Cost;


        protected override bool IsButtonCompatible() => capacityData != null;

        protected override bool Build()
        {
            if (capacityData != null)
            {
                capacityName.text = capacityData.Name;
                description.text = capacityData.Description;
            }

            return base.Build();
        }

        protected override void OnClick(BaseEventData baseEventData)
        {
            RuntimeEntity.PrepareForCapacityCast(capacityData);
        }
    }
}