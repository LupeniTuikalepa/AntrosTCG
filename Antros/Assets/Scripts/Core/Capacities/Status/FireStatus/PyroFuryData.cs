using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Status.FireStatus
{
	[CreateAssetMenu(menuName = "ATCG/Status/PyroFury")]
	public class PyroFuryData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int AttackBuff { get; private set; }
		
		[field: SerializeField, BoxGroup("Specific")]
		public int Duration { get; private set; }
	}
}