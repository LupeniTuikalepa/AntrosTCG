using Helteix.Cards.UI.Physical.Components;
using UnityEngine;

namespace ATCG.Cards.UI.Components
{
    public class CardCapacitiesUI : CardUIComponent<IGameCard>
    {
        [SerializeField]
        private BaseCapacityProviderUI[] providerUis;


        public override void Connect(IGameCard current)
        {
            for (int i = 0; i < providerUis.Length; i++)
            {
                var provider = providerUis[i];
                bool active = provider.Build(current.CardData.Capacities);
                provider.gameObject.SetActive(active);
            }

            base.Connect(current);
        }

        public override void Disconnect(IGameCard current)
        {
            for (int i = 0; i < providerUis.Length; i++)
            {
                var provider = providerUis[i];
                provider.Clear();
                provider.gameObject.SetActive(false);
            }

            base.Disconnect(current);
        }
    }
}