using System;
using System.Collections.Generic;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector
{
    public class EntityInspectorTabContainer : MonoBehaviour
    {
        [SerializeField]
        private EntityInspectorTabButton tabButtonPrefab;

        [SerializeField]
        private Transform tabContainer;
        private EntityInspectorController controller;

        private Dictionary<EntityInspectorTab, EntityInspectorTabButton> tabButtons = new();

        private void Awake()
        {
            controller = GetComponentInParent<EntityInspectorController>();
            tabContainer.ClearChildren();
        }

        public void AddTab(EntityInspectorTab tab)
        {
            EntityInspectorTabButton instance = Instantiate(tabButtonPrefab, tabContainer);
            instance.Clicked += _ => controller.OpenTab(tab);

            instance.Connect(tab);
            tabButtons.Add(tab, instance);

            ShowIfMoreThanOne();
        }

        public void RemoveTab(EntityInspectorTab tab)
        {
            if (tabButtons.Remove(tab, out EntityInspectorTabButton button))
            {
                button.Disconnect(tab);
                Destroy(button.gameObject);
                ShowIfMoreThanOne();
            }
        }

        private void ShowIfMoreThanOne()
        {
            gameObject.SetActive(tabButtons.Count > 1);
        }
    }
}