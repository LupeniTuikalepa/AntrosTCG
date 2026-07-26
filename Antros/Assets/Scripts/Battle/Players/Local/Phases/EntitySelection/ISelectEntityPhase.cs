using System;
using ATCG.Battle.Entities;
using ATCG.Battle.Players.Local.UI;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ISelectEntityPhase : ISinglePhase, IEntitySelectionController, ILocalHUDPhase
    {
        string ISinglePhase.Channel => "SelectPhaseChannel";

        public event Action<ISelectEntityPhase> OnPreviewChanged;

        public event Action<EntityAddress> OnEntitySelected;
        public event Action<EntityAddress> OnEntityUnselected;
        public event Action<EntityAddress> OnEntityHovered;
        public event Action<EntityAddress> OnEntityUnhovered;

        bool IsInPattern(EntityAddress address);
        bool Accepts(EntityAddress address);
        bool IsRelated(EntityAddress address);
        bool IsInPreview(EntityAddress address);

        // The base highlight category for this entity right now (drives the rendering layer it gets).
        // While it's a potential target under the hovered cell (IsInPreview), the listener overrides
        // this with Preview6 instead.
        ATCG.Metrics.HighlightState GetHighlightState(EntityAddress address);
    }
}