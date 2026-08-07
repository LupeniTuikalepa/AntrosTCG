using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Status
{
	[CreateAssetMenu(menuName = "ATCG/Status/Thunder/SpeedUpData")]
	public class SpeedUpStatusData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int SpeedUp { get; private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public int NormalDuration { get; private set; } = 0;
		
		[field: SerializeField, BoxGroup("Specific")]
		public int MaxStack { get; private set; }
	}
}