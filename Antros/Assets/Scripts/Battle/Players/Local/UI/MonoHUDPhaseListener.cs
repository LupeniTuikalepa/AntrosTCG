using System;
using System.Collections.Generic;
using ATCG.Utilities;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using Helteix.Tools.TypeMapping;
using UnityEngine;

namespace ATCG.Battle.Players.Local.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MonoHUDPhaseListener : MonoBehaviour,
        IPhaseListener<IHUDPhase>
    {
        [SerializeField]
        private CanvasGroup group;
        [SerializeField, TypeRefOf(typeof(IHUDPhase))] 
        private List<TypeRef> phaseToHide = new List<TypeRef>();
        [SerializeField]
        private float fadeDuration = .15f;

        private Priority<bool> isVisible;


        private void Reset()
        {
            group = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            isVisible = new Priority<bool>(true);
            isVisible.AddOnValueChangeCallback(HideGroupPriorityOnOnValueChanged, true);
        }

        private void HideGroupPriorityOnOnValueChanged(bool show)
        {
            if (show)
                group.Show(fadeDuration);
            else
                group.Hide(fadeDuration);
        }

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        protected virtual bool GetValueFor(IHUDPhase phase) => false; //TODO faire en sorte que la methode trie les phases

        protected virtual PriorityTags GetPriorityFor(IHUDPhase phase) => PriorityTags.Small;


        void IPhaseListener<IHUDPhase>.OnPhaseBegin(IHUDPhase phase)
        {
            foreach (var type in phaseToHide)
            {
                if (!type.IsAssignableFrom(phase.GetType()))
                    continue;
                
                isVisible.AddPriority(phase.ChannelKey, GetPriorityFor(phase), GetValueFor(phase));
                return;
            }
        }


        void IPhaseListener<IHUDPhase>.OnPhaseEnd(IHUDPhase phase)
        {
            isVisible.RemovePriority(phase.ChannelKey);
        }
    }
}