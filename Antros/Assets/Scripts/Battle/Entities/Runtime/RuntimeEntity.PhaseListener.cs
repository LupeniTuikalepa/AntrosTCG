using ATCG.Battle.Players.Local.Phases;
using ATCG.Metrics;
using Helteix.Tools;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract partial class RuntimeEntity<T> : ILocalPlayerPhaseListener<ISelectEntityPhase>
    {
        public ISelectEntityPhase CurrentSelectEntityPhase { get; private set; }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseBegin(ISelectEntityPhase phase)
        {
            //Debug.Log(((IPhaseListener<ISelectEntityPhase>)this).Accepts(phase));
            CurrentSelectEntityPhase = phase;

            GameMetrics gameMetrics = GameMetrics.Current;

            if (phase.IsInPattern(Address))
            {
                IsInteractable.AddCondition(phase.ChannelKey, true);

                bool isRelated = phase.IsRelated(Address);
                bool accepts = phase.Accepts(Address);

                if (accepts || isRelated)
                {
                    foreach (var model in Models)
                    {
                        model.EnableRenderingLayer(gameMetrics.PhaseSelectableRenderingLayer);
                        model.DisableRenderingLayer(gameMetrics.PhaseRelatedRenderingLayer);
                        model.DisableRenderingLayer(gameMetrics.PhaseUnselectableRenderingLayer);
                    }
                }
                else
                {
                    foreach (var model in Models)
                    {
                        model.DisableRenderingLayer(gameMetrics.PhaseSelectableRenderingLayer);
                        model.EnableRenderingLayer(gameMetrics.PhaseRelatedRenderingLayer);
                        model.DisableRenderingLayer(gameMetrics.PhaseUnselectableRenderingLayer);
                    }
                }
            }
            else
            {
                IsInteractable.AddCondition(phase.ChannelKey, false);
                foreach (var model in Models)
                {
                    model.DisableRenderingLayer(gameMetrics.PhaseSelectableRenderingLayer);
                    model.DisableRenderingLayer(gameMetrics.PhaseRelatedRenderingLayer);
                    model.EnableRenderingLayer(gameMetrics.PhaseUnselectableRenderingLayer);
                }
            }
        }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseEnd(ISelectEntityPhase phase)
        {
            IsInteractable.RemoveCondition(phase.ChannelKey);
            CurrentSelectEntityPhase = null;

            foreach (var model in Models)
            {
                model.DisableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
                model.DisableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
                model.DisableRenderingLayer(GameMetrics.Current.PhaseRelatedRenderingLayer);
            }
        }
    }
}