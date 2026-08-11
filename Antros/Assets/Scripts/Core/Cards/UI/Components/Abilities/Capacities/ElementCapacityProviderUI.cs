using System.Linq;
using ATCG.Elements;
using ATCG.Elements.UI;
using UnityEngine;

namespace ATCG.Cards.UI.Components
{
    public class ElementCapacityProviderUI : BaseCapacityProviderUI
    {
        [SerializeField]
        private ElementUIList elementUIList;


        public override bool Build(ICapacityDataProvider provider)
        {
            if (provider is not ElementCapacityProvider elementCapacityProvider)
            {
                elementUIList.Disconnect();
                return false;
            }

            elementUIList.Connect(
                elementCapacityProvider.Elements.Select(ctx =>
                {
                   if(ctx.TryGetData(out var data))
                       return data;

                   return null;
                }).Where(ctx => ctx is not null));
            return elementUIList.UIItems.Count > 0;
        }

        public override void Clear()
        {
            elementUIList.Disconnect();
        }
    }
}