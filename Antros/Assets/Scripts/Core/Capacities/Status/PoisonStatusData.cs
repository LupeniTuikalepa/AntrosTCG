using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
    [CreateAssetMenu(menuName = "ATCG/Status/Poison")]
    public class PoisonStatusData : StatusData
    {
        [field: SerializeField, BoxGroup("Specific")]
        public int Duration { get; private set; }

        [field: SerializeField, BoxGroup("Specific")]
        public int Damage { get; private set; }
    }
}