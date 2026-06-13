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
    public class MonoHUDPhaseListener : LocalPlayerMonoPhaseListener<IHUDPhase>
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

        protected virtual bool GetValueFor(IHUDPhase phase) =>
            false; //TODO faire en sorte que la methode trie les phases

        protected virtual PriorityTags GetPriorityFor(IHUDPhase phase) => PriorityTags.Small;


        protected override void OnPhaseBegin(IHUDPhase phase)
        {
            foreach (var type in phaseToHide)
            {
                if (!type.IsAssignableFrom(phase.GetType()))
                    continue;

                isVisible.AddPriority(phase.ChannelKey, GetPriorityFor(phase), GetValueFor(phase));
                return;
            }
        }

        protected override void OnPhaseEnd(IHUDPhase phase)
        {
            isVisible.RemovePriority(phase.ChannelKey);

        }
    }
}