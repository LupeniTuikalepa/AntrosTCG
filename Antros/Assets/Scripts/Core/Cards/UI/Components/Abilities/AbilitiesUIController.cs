using System;
using System.Collections.Generic;
using ATCG.Cards;
using ATCG.Cards.UI.Components.Abilities.Tabs;
using ATCG.Tabs;
using Helteix.Cards.UI.Physical.Components;
using Helteix.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG
{
    public class AbilitiesUIController : CardUIComponent<IGameCard>
    {
        [ShowInInspector]
        public int ActiveTabIndex => activeTabs[CurrentActiveTabIndex];
        [ShowInInspector]
        public AbilitiesUITab CurrentTab => tabs[ActiveTabIndex];

        [SerializeField]
        private AbilitiesUITab[] tabs;
        [SerializeField]
        private AbilitiesTabTitle tabTitle;

        [SerializeField]
        private AbilitiesTabButton buttonPrefab;
        [SerializeField]
        private Transform buttonContainer;


        [ShowInInspector]
        private List<int> activeTabs = new List<int>();

        [ShowInInspector, ReadOnly]
        public int CurrentActiveTabIndex { get; private set; }

        private List<AbilitiesTabButton> buttons;

        protected override void Awake()
        {
            buttons = new List<AbilitiesTabButton>();
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].gameObject.SetActive(true);
            }
        }

        public override void Connect(IGameCard current)
        {
            base.Connect(current);

            for (int i = 0; i < tabs.Length; i++)
            {
                var tab = tabs[i];

                if (tab.Build(current))
                {
                    AbilitiesTabButton button = buttonPrefab.InstantiatePrefab(buttonContainer);
                    button.Connect(this, tab);
                    tab.Activate();

                    activeTabs.Add(i);
                    buttons.Add(button);
                }
                else
                {
                    tab.Deactivate();
                }
            }

            if (activeTabs.Count == 0)
                tabTitle.Hide();
            else
            {
                OpenTab(tabs[activeTabs[0]]);
                tabTitle.Show(CurrentTab);
            }

            bool manyTabs = activeTabs.Count > 1;
            buttonContainer.gameObject.SetActive(manyTabs);
        }

        public override void Disconnect(IGameCard current)
        {
            base.Disconnect(current);
            activeTabs.Clear();
            foreach (var button in buttons)
            {
                button.Disconnect();
            }

            buttonContainer.ClearChildren();
        }

        public void OpenTab(AbilitiesUITab tabToOpen)
        {
            for (int i = 0; i < activeTabs.Count; i++)
            {
                int activeTab = activeTabs[i];
                AbilitiesUITab tab = tabs[activeTab];
                if (tab == tabToOpen)
                {
                    tabTitle.Show(tab);
                    tab.Open();
                    CurrentActiveTabIndex = i;
                }
                else
                {
                    tab.Close();
                }
            }
        }

        public void OpenNext()
        {
            CurrentActiveTabIndex++;
            if (CurrentActiveTabIndex >= activeTabs.Count)
                CurrentActiveTabIndex = 0;

            OpenTab(tabs[ActiveTabIndex]);
        }
    }

}