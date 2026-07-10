using System.Collections.Generic;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Utilities;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases.Listeners;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector
{
    public class EntityInspectorController : MonoPhaseListener<InspectEntityPhase> ,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField]
        private EntityInspectorTab[] tabs;
        [SerializeField]
        private EntityInspectorTabContainer tabContainer;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [Header("Setting")]
        [SerializeField]
        private Vector2 offset;

        private RuntimeLocalBattlePlayer runtimeLocalBattlePlayer;
        public Camera OutputCamera => runtimeLocalBattlePlayer.Camera.Component.OutputCamera;


        public int OpenedTabIndex { get; private set; }
        public EntityInspectorTab OpenedTab => tabs[OpenedTabIndex];

        private InspectEntityPhase current;
        
        private List<EntityInspectorTab> activeTabs = new();

        private void Awake()
        {
            runtimeLocalBattlePlayer = GetComponentInParent<RuntimeLocalBattlePlayer>();
            canvasGroup.Hide(0);
        }

        private void Update()
        {
            if (current != null)
            {
                Vector3 pos = current.RuntimeEntity.HoveredRoot.position;
                Vector2 position = OutputCamera.WorldToScreenPoint(pos);
                canvasGroup.Show(.2f);
                transform.position = Vector3.Lerp(transform.position, position + offset, Time.deltaTime * 25f);
            }
        }

        protected override void OnPhaseBegin(InspectEntityPhase phase)
        {
            base.OnPhaseBegin(phase);

            activeTabs.Clear();
            current = phase;
            bool hasContent = false;
            for (int i = 0; i < tabs.Length; i++)
            {
                EntityInspectorTab tab = tabs[i];
                bool connect = tab.Connect(phase);
                if (connect)
                {
                    activeTabs.Add(tab);
                    tabContainer.AddTab(tab);
                }
                else
                {
                    tab.Close();
                }

                hasContent |= connect;
            }

            if (hasContent)
            {
                Vector3 pos = phase.RuntimeEntity.HoveredRoot.position;

                Vector2 position = OutputCamera.WorldToScreenPoint(pos);
                transform.position = position;
                canvasGroup.Show(.2f);
            }

            OpenTab(0);
        }

        protected override void OnPhaseEnd(InspectEntityPhase phase)
        {
            current = null;
            canvasGroup.Hide(.2f);
            phase.isActive.RemovePriority(this);

            for (int i = 0; i < tabs.Length; i++)
            {
                EntityInspectorTab tab = tabs[i];

                tab.Disconnect(phase);
                tabContainer.RemoveTab(tab);
            }

            Tween.Position(transform, transform.position - (Vector3)offset, .2f);

            base.OnPhaseEnd(phase);
        }

        public void NextTab()
        {
            int index = OpenedTabIndex + 1;
            if(index >= activeTabs.Count)
                index = 0;

            OpenTab(index);
        }

        public void PreviousTab()
        {
            int index = OpenedTabIndex - 1;
            if(index < 0)
                index =  activeTabs.Count - 1;

            OpenTab(index);
        }

        public void OpenTab(EntityInspectorTab tab) => OpenTab(activeTabs.IndexOf(tab));
        public void OpenTab(int index)
        {
            for (int i = 0; i < activeTabs.Count; i++)
            {
                if(i == index)
                    tabs[i].Open();
                else
                    tabs[i].Close();
            }

            OpenedTabIndex = index;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if(current != null)
                current.isActive.AddPriority(this, PriorityTags.High, true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (current != null)
                current.isActive.RemovePriority(this);
        }
    }
}