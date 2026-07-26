using ATCG.Battle.Entities.Runtime.VFX;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Metrics;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract partial class RuntimeEntity<T> : ILocalPlayerPhaseListener<ISelectEntityPhase>
    {
        public ISelectEntityPhase CurrentSelectEntityPhase { get; private set; }

        // Category slots this listener drives. The active phase decides which one applies (via
        // GetHighlightState); the mask for each comes from GameMetrics.GetHighlightLayer.
        private static readonly HighlightState[] CategoryStates =
        {
            HighlightState.Preview1,
            HighlightState.Preview2,
            HighlightState.Preview3,
            HighlightState.Preview4,
            HighlightState.Preview5,
            HighlightState.Preview6,
        };

        void IPhaseListener<ISelectEntityPhase>.OnPhaseBegin(ISelectEntityPhase phase)
        {
            CurrentSelectEntityPhase = phase;
            phase.OnPreviewChanged += UpdatePreviewState;
            IsInteractable.AddCondition(phase.ChannelKey, phase.IsInPattern(Address));
            RefreshState(phase);
        }

        // Potential targets under the hovered cell (the phase's preview, e.g. a capacity's hit pattern)
        // get Preview6; otherwise the base state from GetHighlightState applies.
        private void UpdatePreviewState(ISelectEntityPhase phase) => RefreshState(phase);

        private void RefreshState(ISelectEntityPhase phase)
        {
            HighlightState state = phase.IsInPreview(Address)
                ? HighlightState.Preview6
                : phase.GetHighlightState(Address);
            ApplyHighlightState(state);
        }

        private void ApplyHighlightState(HighlightState state)
        {
            GameMetrics gameMetrics = GameMetrics.Current;
            foreach (Renderer renderer in Models)
            {
                for (int i = 0; i < CategoryStates.Length; i++)
                {
                    RenderingLayerMask mask = gameMetrics.GetHighlightLayer(CategoryStates[i]);
                    if (CategoryStates[i] == state)
                        renderer.EnableRenderingLayer(mask);
                    else
                        renderer.DisableRenderingLayer(mask);
                }
            }
        }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseEnd(ISelectEntityPhase phase)
        {
            phase.OnPreviewChanged -= UpdatePreviewState;
            IsInteractable.RemoveCondition(phase.ChannelKey);
            CurrentSelectEntityPhase = null;
            ApplyHighlightState(HighlightState.None);
        }
    }
}