using System.Collections.Generic;
using ATCG.Battle.Entities.Runtime.Heroes.UI.Buttons;
using ATCG.Capacities;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes.UI
{
    public class HeroCapacityGroupUI : MonoBehaviour, IHeroUIPanelElement
    {
        [SerializeField]
        private Transform container;

        [SerializeField]
        private HeroCapacityButtonUI capacityButtonsUI;

        private readonly List<HeroCapacityButtonUI> capacityButtonUis = new();


        private void Awake()
        {
            container.ClearChildren();
        }


        void IHeroUIPanelElement.OnOpen(RuntimeHero runtimeHero, HeroUIPanel panel)
        {
            container.ClearChildren();
            capacityButtonUis.Clear();

            foreach (CapacityData capacity in runtimeHero.Hero.Card.CapacitiesData)
            {
                HeroCapacityButtonUI instance = capacityButtonsUI.InstantiatePrefab(container);
                capacityButtonUis.Add(instance);

                instance.SetCapacity(capacity, runtimeHero, panel);
            }
        }

        void IHeroUIPanelElement.OnClose(RuntimeHero hero, HeroUIPanel panel)
        {
            foreach (HeroCapacityButtonUI capacityButtonUi in capacityButtonUis)
                capacityButtonUi.OnClose(hero, panel);

            container.ClearChildren();
        }
    }
}