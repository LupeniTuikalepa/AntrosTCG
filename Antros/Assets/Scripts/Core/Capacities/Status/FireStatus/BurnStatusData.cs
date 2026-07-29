using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Status.FireStatus
{
	[CreateAssetMenu(menuName = "ATCG/Status/BurnData")]
    public class BurnStatusData : StatusData
    {
		[field: SerializeField, BoxGroup("Specific")]
		public int Damage { get; private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public int normalDuration { get; private set; } = 2;
    }
}
