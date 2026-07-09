using System.Collections.Generic;
using ATCG.Capacities.Attributs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Frost
{
    [CreateAssetMenu(menuName = "ATCG/Capacities/Frost/NorthWall")]
    [WithStep("Construction")]
    public partial class NorthWallData : CapacityData
    {
        [field: SerializeField, BoxGroup("Custom")]
        public int Radius { get; private set; }
        
        [field: SerializeField, BoxGroup("Custom")]
        public DeployableData DeployableData { get; private set; }
    }
}