using ATCG.Battle.Heroes.Runtime;
using ATCG.Capacities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Heroes.Deployed
{
    public class HeroCapacityButtonUI : HeroButtonUI, ILayoutElement
    {
        [SerializeField]
        private RectTransform target;
        [SerializeField]
        private RectTransform source;

        [SerializeField]
        private TMP_Text title;
        [SerializeField]
        private TMP_Text description;

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
        }

        float ILayoutElement.minWidth => source.sizeDelta.x;

        float ILayoutElement.preferredWidth => source.sizeDelta.x;

        float ILayoutElement.flexibleWidth => 0;

        float ILayoutElement.minHeight => source.sizeDelta.y;

        float ILayoutElement.preferredHeight => source.sizeDelta.y;

        float ILayoutElement.flexibleHeight => 0;

        int ILayoutElement.layoutPriority => 1;

        public CapacityData CurrentCapacity { get; private set; }

        protected override int GetCost()
        {
            return CurrentCapacity.Cost;
        }

        public override void OnClick()
        {
            if(RuntimeHero)
                RuntimeHero.UseCapacity(CurrentCapacity);
        }

        public void SetCapacity(CapacityData capacity, RuntimeHero hero, HeroUIPanel panel)
        {
            CurrentCapacity = capacity;
            title.text = capacity.Name;
            description.text = capacity.Description;
            ((IHeroUIPanelElement)this).OnOpen(hero, panel);
        }

        public void OnClose(RuntimeHero hero, HeroUIPanel panel) =>
            ((IHeroUIPanelElement)this).OnClose(hero, panel);
    }
}