using UnityEngine;

namespace ATCG.Tabs
{
    public class AbilitiesTabButton : MonoBehaviour
    {
        [SerializeField]
        private GameObject activeImage;


        private AbilitiesUIController controller;
        private AbilitiesUITab connectedTab;


        public void Connect(AbilitiesUIController abilitiesUIController, AbilitiesUITab tab)
        {
            controller = abilitiesUIController;
            connectedTab = tab;

            tab.OnOpen += OnOpen;
            tab.OnClose += OnClose;
        }



        public void Disconnect()
        {
            if (connectedTab)
            {
                connectedTab.OnOpen -= OnOpen;
                connectedTab.OnClose -= OnClose;
            }

            controller = null;
            connectedTab = null;
        }

        public void OnClick()
        {
            controller.OpenTab(connectedTab);
        }

        private void OnOpen()
        {
            activeImage.SetActive(true);
        }

        private void OnClose()
        {
            activeImage.SetActive(false);
        }
    }
}