using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;

namespace ATCG.Battle
{
    public interface IEntitySelectionController
    {
        int MaxSelectableEntities { get; }

        void OnHoverBegin(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity);
        void OnHoverEnd(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity);
        void OnSelected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity);

        void OnUnselected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity);
    }
}