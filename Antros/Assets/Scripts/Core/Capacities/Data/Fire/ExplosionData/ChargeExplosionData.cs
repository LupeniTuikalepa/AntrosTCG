using ATCG.Capacities.Attributs;
using ATCG.Capacities.Status.FireStatus;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Fire.ExplosionData
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Fire/ChargeExplosionData")]
	[WithStep("Charging")]
	public partial class ChargeExplosionData : CapacityData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public ExplosionStatusData Status {get; private set;}
		
		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; }
	}
}