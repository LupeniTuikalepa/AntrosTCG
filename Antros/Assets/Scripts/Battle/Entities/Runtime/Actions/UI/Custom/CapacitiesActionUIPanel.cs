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


        public override void Build()
        {
            buttonContainer.ClearChildren();

            if (RuntimeEntity.Address.TryGetComponent<CapacityCasterComponent>(out var componentRef))
            {
                CapacityData[] datas = componentRef.GetValue().capacities;
                for (int i = 0; i < datas.Length; i++)
                {
                    CapacityData capacity = datas[i];
                    CapacityButton button = buttonPrefab.InstantiatePrefab(buttonContainer);

                    button.gameObject.name = capacity.Name;
                    button.SetCapacity(capacity);
                }
            }

            CollectButtons();
            base.Build();
        }
    }
}