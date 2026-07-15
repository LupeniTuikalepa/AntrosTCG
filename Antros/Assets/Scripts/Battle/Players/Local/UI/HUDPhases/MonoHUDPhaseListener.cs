using System;
using System.Collections.Generic;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Utilities;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using Helteix.Tools.TypeMapping;
using UnityEngine;

namespace ATCG.Battle.Players.Local.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MonoHUDPhaseListener : LocalPlayerMonoPhaseListener<ILocalHUDPhase>, IPhaseListener<IGlobalHUDPhase>
    {
        [SerializeField]
        private CanvasGroup group;

        [SerializeField, TypeRefOf(typeof(IBaseHUDPhase))]
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


        protected override void OnEnable()
        {
            base.OnEnable();
            PhaseManager.Register<IGlobalHUDPhase>(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PhaseManager.Unregister<IGlobalHUDPhase>(this);
        }

        private void HideGroupPriorityOnOnValueChanged(bool show)
        {
            if (show)
                group.Show(fadeDuration);
            else
                group.Hide(fadeDuration);
        }

        protected virtual bool GetValueFor(IBaseHUDPhase phase) =>
            false; //TODO faire en sorte que la methode trie les phases

        protected virtual PriorityTags GetPriorityFor(IBaseHUDPhase phase) => PriorityTags.Small;


        protected override void OnPhaseBegin(ILocalHUDPhase phase)
        {
            OnBasePhaseBegin(phase);
        }

        protected override void OnPhaseEnd(ILocalHUDPhase phase)
        {
            OnBasePhaseEnd(phase);
        }

        void IPhaseListener<IGlobalHUDPhase>.OnPhaseBegin(IGlobalHUDPhase phase)
        {
            OnBasePhaseBegin(phase);
        }

        void IPhaseListener<IGlobalHUDPhase>.OnPhaseEnd(IGlobalHUDPhase phase)
        {
            OnBasePhaseEnd(phase);
        }

        private void OnBasePhaseBegin(IBaseHUDPhase phase)
        {
            if (phaseToHide.Count == 0)
            {
                isVisible.AddPriority(phase.ChannelKey, GetPriorityFor(phase), GetValueFor(phase));
            }
            foreach (var type in phaseToHide)
            {
                if (!type.IsAssignableFrom(phase.GetType()))
                    continue;

                isVisible.AddPriority(phase.ChannelKey, GetPriorityFor(phase), GetValueFor(phase));
                return;
            }
        }
        private void OnBasePhaseEnd(IBaseHUDPhase phase)
        {
            isVisible.RemovePriority(phase.ChannelKey);
        }

    }
}