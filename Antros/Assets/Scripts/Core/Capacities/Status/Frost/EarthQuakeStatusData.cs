using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
	[CreateAssetMenu(menuName = "ATCG/Status/Frost/EarthQuakeStatusData")]
	public class EarthQuakeStatusData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int DamagePercentage{ get; private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public int Duration { get; private set; } = 2;
	}
}