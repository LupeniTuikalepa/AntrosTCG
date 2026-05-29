using System;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract partial class RuntimeEntity<T> : IPhaseListener<ISelectEntityPhase>
    {
        public ISelectEntityPhase CurrentSelectEntityPhase { get; private set; }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseBegin(ISelectEntityPhase phase)
        {
            CurrentSelectEntityPhase = phase;
            IsInteractable.AddCondition(phase.ChannelKey, phase.Accepts(Address));
            UpdateRenderers();
        }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseEnd(ISelectEntityPhase phase)
        {
            CurrentSelectEntityPhase = null;
        }

    }
}