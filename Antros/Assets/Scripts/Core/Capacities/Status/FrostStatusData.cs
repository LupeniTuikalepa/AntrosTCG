using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
    [CreateAssetMenu(menuName = "ATCG/Status/Frost")]
    public class FrostStatusData : StatusData
    {
        [field: SerializeField]
        public int Duration { get; private set; }
    }
}