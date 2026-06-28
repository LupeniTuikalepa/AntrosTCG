using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
    [CreateAssetMenu(menuName = "ATCG/Status/Poison")]
    public class PoisonStatusData : StatusData
    {
        [field: SerializeField]
        public int Duration { get; private set; }
        
        [field: SerializeField]
        public int Damage { get; private set; }
    }
}