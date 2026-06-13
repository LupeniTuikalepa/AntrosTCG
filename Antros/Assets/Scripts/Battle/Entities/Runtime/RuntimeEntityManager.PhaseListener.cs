using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Entities.Runtime
{
    public partial class RuntimeEntityManager : ILocalPlayerPhaseListener<ISelectEntityPhase>
    {
        LocalBattlePlayer ILocalPlayerPhaseListener<ISelectEntityPhase>.LocalBattlePlayer => LocalBattlePlayer;

        void IPhaseListener<ISelectEntityPhase>.OnPhaseBegin(ISelectEntityPhase phase)
        {
            ClearSelection();
            SelectionController.AddPriority(phase.ChannelKey, PriorityTags.Highest, phase);
        }

        void IPhaseListener<ISelectEntityPhase>.OnPhaseEnd(ISelectEntityPhase phase)
        {
            ClearSelection();
            SelectionController.RemovePriority(phase.ChannelKey);
        }

    }
}