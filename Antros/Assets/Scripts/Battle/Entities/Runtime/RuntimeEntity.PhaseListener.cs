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
            IsInteractable.AddCondition(phase.ChannelKey, phase.IsInPattern(Address));
            ApplyHighlightState(phase.GetHighlightState(Address));
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
            IsInteractable.RemoveCondition(phase.ChannelKey);
            CurrentSelectEntityPhase = null;
            ApplyHighlightState(HighlightState.None);
        }
    }
}