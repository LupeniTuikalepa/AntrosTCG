using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Data.Status.Life;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Life
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Life/CarnageSustainData")]
	[WithStep("DeploySustance")]
	[WithStep("Punch")]
	public partial class CarnageSustainData : CapacityData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public SustainStatusData Status { get; private set; }

		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; } = 4;

		[field: SerializeField, BoxGroup("Custom")]
		public int Damage { get; private set; } = 5;
	}
}