using System.Collections.Generic;
using ATCG.Battle.Heroes.Runtime;
using ATCG.Capacities;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Heroes.Deployed
{
    public class HeroCapacityGroupUI :MonoBehaviour, IHeroUIPanelElement
    {
        [SerializeField]
        private Transform container;

        [SerializeField]
        private HeroCapacityButtonUI capacityButtonsUI;

        private List<HeroCapacityButtonUI> capacityButtonUis = new List<HeroCapacityButtonUI>();


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
            foreach (var capacityButtonUi in capacityButtonUis)
                capacityButtonUi.OnClose(hero, panel);

            container.ClearChildren();
        }
    }
}