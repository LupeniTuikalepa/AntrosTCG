using ATCG.Battle.Entities.Aspects;
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
                    Model.EnableRenderingLayer(gameMetrics.PhaseSelectableRenderingLayer);
                    Model.DisableRenderingLayer(gameMetrics.PhaseRelatedRenderingLayer);
                    Model.DisableRenderingLayer(gameMetrics.PhaseUnselectableRenderingLayer);
                    if(accepts)
                        Model.DisableRenderingLayer(gameMetrics.DitherOccluderRenderingLayer);
                    else
                        Model.EnableRenderingLayer(gameMetrics.DitherOccluderRenderingLayer);
                }
                else
                {
                    Model.DisableRenderingLayer(gameMetrics.PhaseSelectableRenderingLayer);
                    Model.EnableRenderingLayer(gameMetrics.PhaseRelatedRenderingLayer);
                    Model.DisableRenderingLayer(gameMetrics.PhaseUnselectableRenderingLayer);
                    Model.DisableRenderingLayer(gameMetrics.DitherOccluderRenderingLayer);
                }
            }
            else
            {
                IsInteractable.AddCondition(phase.ChannelKey, false);

                Model.DisableRenderingLayer(gameMetrics.PhaseSelectableRenderingLayer);
                Model.DisableRenderingLayer(gameMetrics.PhaseRelatedRenderingLayer);
                Model.EnableRenderingLayer(gameMetrics.PhaseUnselectableRenderingLayer);
                Model.EnableRenderingLayer(gameMetrics.DitherOccluderRenderingLayer);
            }
        }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseEnd(ISelectEntityPhase phase)
        {
            IsInteractable.RemoveCondition(phase.ChannelKey);
            CurrentSelectEntityPhase = null;

            Model.DisableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
            Model.DisableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
            Model.DisableRenderingLayer(GameMetrics.Current.PhaseRelatedRenderingLayer);
            Model.DisableRenderingLayer(GameMetrics.Current.DitherOccluderRenderingLayer);
        }
    }
}