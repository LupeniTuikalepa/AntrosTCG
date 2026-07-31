using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Frost
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Frost/Stalemate")]
	[WithStep("Stalemate")]
	public partial class StalemateData : CapacityData
	{
		[field:SerializeField,BoxGroup("Specific")]
		public SlowDownStatusData Status { get;private set; }
        		
		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; }
	}
}