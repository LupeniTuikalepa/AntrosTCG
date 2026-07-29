using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Status.FireStatus
{
	[CreateAssetMenu(menuName = "ATCG/Status/Fire/Incandescence")]
	public class IncandescenceData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int Duration { get; private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public BurnStatusData Status { get; private set; }
	}
}