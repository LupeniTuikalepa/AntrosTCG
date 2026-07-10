using ATCG.Capacities.Attributs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Fire
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Fire/BuildBoiler")]
    [WithStep("Build")]
    public partial class BuildBoilerData : CapacityData
    {
        [field: SerializeField, BoxGroup("Custom")]
        public DeployableData DeployableData { get; private set; }
    }
}