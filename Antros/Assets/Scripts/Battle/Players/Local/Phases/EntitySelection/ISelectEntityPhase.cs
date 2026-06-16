using ATCG.Battle.Entities;
using ATCG.Battle.Players.Local.UI;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ISelectEntityPhase : ISinglePhase, IEntitySelectionController, IHUDPhase
    {
        string ISinglePhase.Channel => "SelectPhaseChannel";

        bool IsInPattern(EntityAddress address);
        bool Accepts(EntityAddress address);
        bool IsRelated(EntityAddress address);
    }
}