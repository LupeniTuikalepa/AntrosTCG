using ATCG.Capacities.Attributs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Fire
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Fire/Devastation")]

    [WithStep("Before Explosion")]
    [WithStep("After")]
    public partial class DevastationData : CapacityData
    {
        [field: SerializeField, BoxGroup("Custom")]
        public int Range { get; private set; }

        [field: SerializeField, BoxGroup("Custom")]
        public int Damage { get; private set; }
    }


}