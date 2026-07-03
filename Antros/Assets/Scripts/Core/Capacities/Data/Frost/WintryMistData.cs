using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Frost
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Frost/WintryMist")]

    [WithStep("BlackIce")]
    public partial class WintryMistData : CapacityData
    {
        [field: SerializeField, BoxGroup("Custom")]
        public int Range { get; private set; }
        
        [field: SerializeField, BoxGroup("Custom")]
        public FrostStatusData Status { get; private set; }
    }
}