using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Entities.Runtime
{
    public partial class RuntimeEntityManager : IPhaseListener<ISelectEntityPhase>
    {
        void IPhaseListener<ISelectEntityPhase>.OnPhaseBegin(ISelectEntityPhase phase)
        {
            ClearSelection();
        }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseEnd(ISelectEntityPhase phase)
        {
            ClearSelection();
        }
    }
}