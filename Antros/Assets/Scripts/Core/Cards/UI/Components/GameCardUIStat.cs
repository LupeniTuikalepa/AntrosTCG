using System;
using ATCG.Cards.Implementations;
using Helteix.Cards.UI.Physical.Components;
using TMPro;
using UnityEngine;

namespace ATCG.Cards.UI.Components
{
    public class GameCardUIStat : CardUIComponent<IGameCard>
    {
        public enum StatType
        {
            Health,
            Speed,
            Strength,
            DeathCost,
            InvocationCost,
        }

        [SerializeField]
        private StatType statType;

        [SerializeField]
        private TMP_Text tmpText;

        protected override void Awake()
        {
            base.Awake();
            gameObject.SetActive(false);
        }

        public override void Connect(IGameCard current)
        {
            base.Connect(current);
            switch (statType, current)
            {
                case (StatType.Health, IHeroCard heroCard):
                    gameObject.SetActive(true);
                    tmpText.text = heroCard.Health.ToString();
                    break;
                case (StatType.DeathCost, IHeroCard heroCard):
                    gameObject.SetActive(true);
                    tmpText.text = heroCard.DeathCost.ToString();
                    break;
                case (StatType.Speed, IHeroCard heroCard):
                    gameObject.SetActive(true);
                    tmpText.text = heroCard.Speed.ToString();
                    break;
                case (StatType.Strength, IHeroCard heroCard):
                    gameObject.SetActive(true);
                    tmpText.text = heroCard.Strength.ToString();
                    break;
                case (StatType.InvocationCost, _):
                    gameObject.SetActive(true);
                    tmpText.text = current.InvocationCost.ToString();

                    break;

                default:
                    break;
            }
        }

        public override void Disconnect(IGameCard current)
        {
            base.Disconnect(current);
        }
    }
}