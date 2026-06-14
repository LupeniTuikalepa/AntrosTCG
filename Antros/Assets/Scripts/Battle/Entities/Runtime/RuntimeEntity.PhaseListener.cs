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

            if (phase.IsInPattern(Address))
            {
                IsInteractable.AddCondition(phase.ChannelKey, true);
                if(Address.Is<BattleCellAspect>())
                    Debug.Log(phase.Accepts(Address));

                if (phase.Accepts(Address))
                {

                    Model.EnableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
                    Model.DisableRenderingLayer(GameMetrics.Current.PhaseRelatedRenderingLayer);
                    Model.DisableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
                }
                else
                {
                    Model.DisableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
                    Model.EnableRenderingLayer(GameMetrics.Current.PhaseRelatedRenderingLayer);
                    Model.DisableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
                }
            }
            else
            {
                IsInteractable.AddCondition(phase.ChannelKey, false);
                Model.DisableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
                Model.DisableRenderingLayer(GameMetrics.Current.PhaseRelatedRenderingLayer);
                Model.EnableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
            }
        }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseEnd(ISelectEntityPhase phase)
        {
            IsInteractable.RemoveCondition(phase.ChannelKey);
            CurrentSelectEntityPhase = null;

            Model.DisableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
            Model.DisableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
            Model.DisableRenderingLayer(GameMetrics.Current.PhaseRelatedRenderingLayer);
        }
    }
}