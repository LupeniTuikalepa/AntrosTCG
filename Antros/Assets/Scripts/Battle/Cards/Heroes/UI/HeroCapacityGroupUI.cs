using System;
using System.Collections.Generic;
using ATCG.Battle.Heroes.Runtime;
using ATCG.Capacities;
using ATCG.Capacities.UI;
using Helteix.Tools;
using Helteix.Tools.UI;
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

        void IHeroUIPanelElement.OnOpen(RuntimeHero hero, HeroUIPanel panel)
        {
            container.ClearChildren();
            capacityButtonUis.Clear();
            foreach (CapacityData capacity in hero.Card.Capacities)
            {
                HeroCapacityButtonUI instance = capacityButtonsUI.InstantiatePrefab(container);
                capacityButtonUis.Add(instance);

                instance.SetCapacity(capacity);
                ((IHeroUIPanelElement)instance).OnOpen(hero, panel);
            }
        }

        void IHeroUIPanelElement.OnClose(RuntimeHero hero, HeroUIPanel panel)
        {
            foreach (var capacityButtonUi in capacityButtonUis)
                ((IHeroUIPanelElement)capacityButtonUi).OnClose(hero, panel);

            container.ClearChildren();
        }
    }
}