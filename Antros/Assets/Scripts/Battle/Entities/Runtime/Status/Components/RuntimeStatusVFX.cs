using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatusVFX : RuntimeStatusComponent
    {
        public override void OnApplyStatus(StatusData statusData)
        {
            Debug.Log("[RuntimeStatusVFX] OnApplyStatus");
        }

        public override void OnRemoveStatus()
        {
            Debug.Log("[RuntimeStatusVFX] OnRemoveStatus");
        }

        public override void OnTickStatus(RuntimeStatusContext context)
        {
            Debug.Log("[RuntimeStatusVFX] OnTickStatus");
        }
    }
}