using System;
using ATCG.Cards;
using ATCG.Enums;
using Helteix.Cards.UI.Physical.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG
{
    public class CardElementUI : CardUIComponent<IGameCard>
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private Element element;

        private void Reset()
        {
            image = GetComponent<Image>();
        }


        public override void Connect(IGameCard current)
        {
            base.Connect(current);
            gameObject.SetActive(current.CardData.Element == element);
        }

        public override void Disconnect(IGameCard current)
        {
            base.Disconnect(current);
            gameObject.SetActive(false);
        }
    }
}