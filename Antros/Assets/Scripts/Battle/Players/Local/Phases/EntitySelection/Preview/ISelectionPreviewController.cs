using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.GameModes;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.Players.Local.Phases.Preview
{
    public interface ISelectionPreviewController
    {
        void FillPreview(ISelectEntityPhase phase, EntityAddress entityAddress, List<EntityAddress> previews);
    }
}