using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Status.Components
{
    public abstract class RuntimeStatusComponent : MonoBehaviour, IRuntimeStatusComponent
    {
        public abstract void OnApplyStatus(RuntimeStatusContext context);

        public abstract void OnRemoveStatus(RuntimeStatusContext context);

        public abstract void OnTickStatus(RuntimeStatusContext context);
    }
}