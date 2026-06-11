using ATCG.Battle.Entities;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ISelectEntityPhase : ISinglePhase, IEntitySelectionController
    {
        string ISinglePhase.Channel => "SelectPhaseChannel";

        ChannelKey ChannelKey { get; }
        bool Accepts(EntityAddress address);
    }
}