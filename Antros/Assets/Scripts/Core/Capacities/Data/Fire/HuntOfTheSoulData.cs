using ATCG.Capacities.Attributs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Fire
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Fire/HuntOfTheSoul")]
    [WithStep("Spawn")]
    public partial class HuntOfTheSoulData : CapacityData
    {
        [field: SerializeField, BoxGroup("Custom")]
        public DeployableData Deployable { get; private set; }

    }
}
