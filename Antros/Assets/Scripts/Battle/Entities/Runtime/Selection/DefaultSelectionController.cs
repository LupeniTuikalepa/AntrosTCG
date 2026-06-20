using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;

namespace ATCG.Battle
{
    public class DefaultSelectionController : IEntitySelectionController
    {
        public int MaxSelectableEntities => 1;
        public void OnHoverBegin(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity) { }
        public void OnHoverEnd(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity) { }

        public void OnSelected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity) { }

        public void OnUnselected(IRuntimeEntity runtimeEntity, ref EntityAddress selectedEntity) { }
    }
}