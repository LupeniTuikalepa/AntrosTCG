using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;

namespace ATCG.Battle
{
    public interface IEntitySelectionController
    {
        int MaxSelectableEntities { get; }

        void OnSelected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity);

        void OnUnselected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity);
    }
}