using Helteix.Cards.UI.Physical.Components;
using UnityEngine;

namespace ATCG.Cards.UI.Components
{
    public class CardCapacitiesUI : AbilitiesUITab
    {
        [SerializeField]
        private BaseCapacityProviderUI[] providerUis;


        public override bool Build(IGameCard gameCard)
        {
            bool any = false;
            for (int i = 0; i < providerUis.Length; i++)
            {
                var provider = providerUis[i];
                bool isActive = provider.Build(gameCard.CardData.Capacities);
                provider.gameObject.SetActive(isActive);

                any |= isActive;
            }

            return any;
        }
        public override void Clear()
        {
            for (int i = 0; i < providerUis.Length; i++)
            {
                var provider = providerUis[i];
                provider.Clear();
                provider.gameObject.SetActive(false);
            }
        }
    }
}