using System.Collections.Generic;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace Helteix.Cards.UI.Physical
{
    public class CardFactoryUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject defaultCardPrefab;

        [SerializeField]
        private Transform root;


        private Dictionary<ICard, ICardUI> cardUIs;

        protected virtual GameObject GetPrefabForCard(ICard card) => defaultCardPrefab;


        private void Awake()
        {
            cardUIs = new Dictionary<ICard, ICardUI>();
        }

        private void LateUpdate()
        {
            //Cleaning cards with no containers.
            using (ListPool<ICard>.Get(out var cards))
            {
                cards.AddRange(cardUIs.Keys);
                foreach (var card in cards)
                {
                    if (card.Container != null)
                        continue;

                    cardUIs.Remove(card, out ICardUI ui);
                    ui.Disconnect();

                    Destroy(ui.gameObject);
                }
            }
        }

        public bool TryGetUIForCard<TCard>(TCard card, out CardUI<TCard> cardUI)

            where TCard : ICard
        {
            if (cardUIs.TryGetValue(card, out ICardUI ui))
            {
                cardUI = ui as CardUI<TCard>;
                return cardUI;
            }

            GameObject prefab = GetPrefabForCard(card);
            GameObject instance = Instantiate(prefab, root);

            instance.SetActive(false);
            
            if (!instance.TryGetComponent(out cardUI))
            {
                Debug.LogError($"No compatible CardUI component found on {prefab.name} prefab. Destroying instance.");
                instance.Destroy();
                return false;
            }

            cardUI.FactoryUI = this;
            cardUIs.Add(card, cardUI);
            return true;
        }

        public void Activate<TCard>(CardUI<TCard> cardUI, Transform snapTo = null)
            where TCard : ICard
        {
            cardUI.gameObject.SetActive(true);
            if (snapTo)
            {
                cardUI.transform.position = snapTo.position;
                cardUI.transform.rotation = snapTo.rotation;
            }

        }
        public void Return<TCard>(CardUI<TCard> cardUI)
            where TCard : ICard
        {
            cardUI.transform.localPosition = Vector3.zero;
            cardUI.gameObject.SetActive(false);
        }
    }
}