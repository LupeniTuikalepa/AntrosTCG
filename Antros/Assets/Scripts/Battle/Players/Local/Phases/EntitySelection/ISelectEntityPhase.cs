using System;
using ATCG.Battle.Entities;
using ATCG.Battle.Players.Local.UI;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ISelectEntityPhase : ISinglePhase, IEntitySelectionController, IHUDPhase
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
    }
}