using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
	[CreateAssetMenu(menuName = "ATCG/Status/Frost/SlowDownData")]
	public class SlowDownStatusData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int Slow { get; private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public int NormalDuration { get; private set; } = 2;
		
		[field: SerializeField, BoxGroup("Specific")]
		public int MaxStack { get; private set; }
	}
}