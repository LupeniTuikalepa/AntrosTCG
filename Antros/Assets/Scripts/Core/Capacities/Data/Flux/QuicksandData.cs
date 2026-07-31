using System.Collections.Generic;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Flux/Quicksand")]
	[WithStep("Quicksand")]
	public partial class QuicksandData : CapacityData
	{
		[field:SerializeField,BoxGroup("Specific")]
		public SlowDownStatusData Status { get;private set; }

		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; } = 1;

		[field: SerializeField, BoxGroup("Custom")]
		public int Damage { get; private set; } = 2;
	}
}