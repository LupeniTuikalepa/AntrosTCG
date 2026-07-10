using System.Collections.Generic;
using ATCG.Capacities.Attributs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Frost
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Frost/IceshardHammer")]
    [WithStep("Destruction")]
    
    public partial class IceshardHammerData : CapacityData
    {
        [field: SerializeField, BoxGroup("Custom")]
        public int Damage { get; private set; }
        
        [field: SerializeField, BoxGroup("Custom")]
        public int PushbackMultiplier { get; private set; }
        
        [field: SerializeField, BoxGroup("Custom")]
        public DeployableData Breakable { get; private set; }
    }
}