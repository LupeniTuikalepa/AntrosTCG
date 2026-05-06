using ATCG.Cards.UI.Components;
using ATCG.UI;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes.UI.Buttons
{
    public abstract class HeroButtonUI : MonoBehaviour, IHeroUIPanelElement
    {
        [SerializeField]
        private ManaCostUI costUI;

        [SerializeField]
        private CustomButtonUI customButtonUI;

        public RuntimeHero RuntimeHero { get; private set; }
        public HeroUIPanel Panel { get; private set; }

        void IHeroUIPanelElement.OnOpen(RuntimeHero runtimeHero, HeroUIPanel panel)
        {
            RuntimeHero = runtimeHero;
            Panel = panel;

            int cost = GetCost();
            costUI.SetCost(cost);
            customButtonUI.Interactable = runtimeHero.Hero.Card.Player.CurrentMana >= cost;
        }

        void IHeroUIPanelElement.OnClose(RuntimeHero runtimeHero, HeroUIPanel panel)
        {
            customButtonUI.Interactable = false;
        }

        protected abstract int GetCost();

        public abstract void OnClick();
    }
}