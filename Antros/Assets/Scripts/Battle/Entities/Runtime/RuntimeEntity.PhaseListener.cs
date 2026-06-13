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
            bool accepts = phase.Accepts(Address);

            IsInteractable.AddCondition(phase.ChannelKey, accepts);

            if (accepts)
            {
                Model.EnableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
                Model.DisableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
            }
            else
            {
                Model.DisableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
                Model.EnableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
            }
        }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseEnd(ISelectEntityPhase phase)
        {
            IsInteractable.RemoveCondition(phase.ChannelKey);
            CurrentSelectEntityPhase = null;

            Model.DisableRenderingLayer(GameMetrics.Current.PhaseSelectableRenderingLayer);
            Model.DisableRenderingLayer(GameMetrics.Current.PhaseUnselectableRenderingLayer);
        }
    }
}