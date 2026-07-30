using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Frost
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Frost/EarthFracture")]
	[WithStep("EarthFracture")]
	public partial class EarthFractureData : CapacityData
	{
		[field:SerializeField,BoxGroup("Specific")]
		public EarthQuakeStatusData Status { get;private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public int Range { get; private set; }

		[field: SerializeField, BoxGroup("Specific"), Min(1)]
		public int EarthQuakePropagationRange { get; private set; } = 1;
	}
}