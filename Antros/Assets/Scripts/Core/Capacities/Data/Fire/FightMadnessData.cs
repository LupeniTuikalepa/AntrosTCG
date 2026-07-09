using ATCG.Capacities;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG
{
	[WithStep("DeployRage")]
	[WithStep("Punch")]
	[CreateAssetMenu (menuName = "ATCG/Capacities/Fire/FightMadnessData")]
    public partial class FightMadnessData : CapacityData
    {
        [field:SerializeField,BoxGroup("Specific")]
        public BerserkStatusData BerserkData { get;private set; }
        
        [field:SerializeField,BoxGroup("Specific")]
        public int PunchDamage { get;private set; }
        
    }
}
