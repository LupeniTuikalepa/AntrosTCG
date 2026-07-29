using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
	[CreateAssetMenu(menuName = "ATCG/Status/Frost/HardeningData")]
	public class HardeningData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int DefenseBuff {get; private set; } = 2;
		[field: SerializeField, BoxGroup("Specific")]
		public int  Duration {get; private set; } = 2;
	}
}