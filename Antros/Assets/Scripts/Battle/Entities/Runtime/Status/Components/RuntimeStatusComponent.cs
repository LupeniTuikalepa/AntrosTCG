using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public abstract class RuntimeStatusComponent : MonoBehaviour, IRuntimeStatusComponent
    {
        public abstract void OnApplyStatus(StatusData statusData);

        public abstract void OnRemoveStatus();

        public abstract void OnTickStatus(RuntimeStatusContext context);
    }
}