using ATCG.Capacities;
using ATCG.Capacities.UI;
using Helteix.Tools.UI;
using UnityEngine;

namespace ATCG.Cards.UI.Components
{

    public class DefaultCapacityProviderUI : BaseCapacityProviderUI
    {
        [SerializeField]
        private CapacityUIList capacityUIList;

        public override bool Build(ICapacityDataProvider provider)
        {
            if (provider is not DefaultCapacityProvider defaultCapacityProvider)
            {
                capacityUIList.Disconnect();
                return false;
            }

            capacityUIList.Connect(defaultCapacityProvider.GetCapacities());
            return capacityUIList.UIItems.Count > 0;
        }

        public override void Clear()
        {
            capacityUIList.Disconnect();
        }
    }
}