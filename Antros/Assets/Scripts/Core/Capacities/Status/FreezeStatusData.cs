using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
    [CreateAssetMenu(menuName = "ATCG/Status/Frost")]
    public class FreezeStatusData : StatusData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public int Duration { get; private set; }
    }
}