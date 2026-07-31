using System.Collections.Generic;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Status.FireStatus;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Fire.ExplosionData
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Fire/KamikazeData")]
	[WithStep("Explose")]
	[WithStep("Die")]
	public partial class KamikazeData : CapacityData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public ExplosionStatusData Status {get; private set;}
		
		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; }
	}
}