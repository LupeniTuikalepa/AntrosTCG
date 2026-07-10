using System;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector
{
    public class EntityInspectorTabButton : MonoBehaviour
    {
        public event Action<EntityInspectorTabButton> Clicked;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Image activeImage;

        public void Connect(EntityInspectorTab tab)
        {
            icon.sprite = tab.TabIcon;
            tab.Opened += OnTabOpened;
            tab.Closed += OnTabClosed;
        }

        public void Disconnect(EntityInspectorTab tab)
        {
            icon.sprite = null;
            tab.Opened -= OnTabOpened;
            tab.Closed -= OnTabClosed;
        }


        private void OnTabOpened(EntityInspectorTab tab)
        {
            activeImage.enabled = true;
            icon.CrossFadeAlpha(1, .2f, true);
        }


        private void OnTabClosed(EntityInspectorTab tab)
        {
            activeImage.enabled = false;
            icon.CrossFadeAlpha(.5f, .2f, true);
        }

        public void OnClick()
        {
            Clicked?.Invoke(this);
        }
    }
}