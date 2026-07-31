using ATCG.Capacities.Attributs;
using ATCG.Capacities.Status.FireStatus;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Fire.ExplosionData
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Fire/GiveOrExplose")]
	[WithStep("Hit")]
	[WithStep("GiveOrExplose")]
	public partial class GiveOrExploseData : CapacityData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int PunchDamage {get; private set;}
		
		[field: SerializeField, BoxGroup("Specific")]
		public ExplosionStatusData Status {get; private set;}
		
		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; }
	}
}