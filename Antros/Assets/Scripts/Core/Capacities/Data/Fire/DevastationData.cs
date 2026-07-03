using ATCG.Capacities.Attributs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Fire
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Fire/Devastation")]

    [WithStep("Explosion")]
    public partial class DevastationData : CapacityData
    {
        [field: SerializeField, BoxGroup("Custom")]
        public int Range { get; private set; }

        [field: SerializeField, BoxGroup("Custom")]
        public AnimationCurve Damage { get; private set; }

    }
}