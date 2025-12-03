using UnityEngine;

namespace ATCG.Capacities.Data
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Debug")]
    public class DebugCapacityData : CapacityData
    {
        [field: SerializeField]
        public string Message { get; private set; }

    }
}