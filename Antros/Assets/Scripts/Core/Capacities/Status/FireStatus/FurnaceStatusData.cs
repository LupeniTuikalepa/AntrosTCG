using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Status.FireStatus
{
	[CreateAssetMenu(menuName = "ATCG/Status/Fournaise")]

	public class FurnaceStatusData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int ManaRemove { get; private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public int Duration { get; private set; }
	}
}