using ATCG.Battle.Entities.Runtime;

namespace ATCG.Battle
{
    public class DefaultSelectionController : IEntitySelectionController
    {
        public int MaxSelectableEntities => 1;

        public void OnSelected(IRuntimeEntity runtimeEntity) { }

        public void OnDeselected(IRuntimeEntity runtimeEntity) { }
    }
}