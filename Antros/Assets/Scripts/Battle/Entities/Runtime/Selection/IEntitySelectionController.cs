using ATCG.Battle.Entities.Runtime;
using UnityEngine;

namespace ATCG.Battle
{
    public interface IEntitySelectionController
    {
        int MaxSelectableEntities { get; }

        void OnSelected(IRuntimeEntity runtimeEntity);
        
        void OnDeselected(IRuntimeEntity runtimeEntity);
    }
}