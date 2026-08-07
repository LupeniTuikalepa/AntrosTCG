using System;
using ATCG.Cards;
using ATCG.Elements;
using ATCG.Elements.UI;
using ATCG.Enums;
using Helteix.Cards.UI.Physical.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG
{
    [RequireComponent(typeof(CardElementUI))]
    public class CardElementUI : CardUIComponent<IGameCard>
    {
        [SerializeField]
        private ElementUI elementUI;

        private void Reset()
        {
            elementUI = GetComponent<ElementUI>();
        }

        public override void Connect(IGameCard current)
        {
            base.Connect(current);
            elementUI.Setup(current.CardData.Element);
        }
    }
}