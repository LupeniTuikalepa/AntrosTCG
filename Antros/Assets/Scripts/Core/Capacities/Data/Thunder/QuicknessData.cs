using ATCG.Capacities.Attributs;
using ATCG.Capacities.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Frost
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Thunder/QuicknessData")]
	[WithStep("Quickness")]
	public partial class QuicknessData : CapacityData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public SpeedUpStatusData Status { get; private set; }
		
		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; }
	}
}