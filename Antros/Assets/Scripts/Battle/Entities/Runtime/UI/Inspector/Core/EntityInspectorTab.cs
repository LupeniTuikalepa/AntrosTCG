using System;
using ATCG.Battle.Players.Local.Phases;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector
{
    public class EntityInspectorTab : MonoBehaviour
    {
        public event Action<EntityInspectorTab> Opened;
        public event Action<EntityInspectorTab> Closed;

        [SerializeField]
        private EntityInspectorTabContainer tabContainer;

        [field: SerializeField]
        public Sprite TabIcon { get; private set; }
        [field: SerializeField]
        public string TabName { get; private set; }

        private EntityInspectorTabElement[] elements;

        private void Start()
        {
            elements = GetComponentsInChildren<EntityInspectorTabElement>();
        }

        public virtual bool Connect(InspectEntityPhase phase)
        {
            bool anyContent = false;
            for (int i = 0; i < elements.Length; i++)
            {
                EntityInspectorTabElement tabElement = elements[i];
                if(tabElement == null)
                    continue;

                if (tabElement.Connect(phase))
                {
                    anyContent = true;
                    tabElement.gameObject.SetActive(true);
                }
                else
                {
                    tabElement.gameObject.SetActive(false);
                }
            }

            return anyContent;
        }

        public virtual void Disconnect(InspectEntityPhase phase)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                EntityInspectorTabElement tabElement = elements[i];
                if(tabElement == null)
                    continue;

                tabElement.Disconnect(phase);
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Opened?.Invoke(this);
        }

        public void Close()
        {
            gameObject.SetActive(false);
            Closed?.Invoke(this);
        }
    }
}