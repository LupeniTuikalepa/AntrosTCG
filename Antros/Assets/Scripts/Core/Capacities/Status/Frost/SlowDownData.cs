using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
	public class SlowDownData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int Slow { get; private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public int NormalDuration { get; private set; } = 2;
		
		[field: SerializeField, BoxGroup("Specific")]
		public int AddDuration { get; private set; } = 1;
	}
}