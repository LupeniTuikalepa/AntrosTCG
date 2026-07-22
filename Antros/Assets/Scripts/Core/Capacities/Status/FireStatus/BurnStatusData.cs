using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG
{
	[CreateAssetMenu(menuName = "ATCG/Status/Flame")]
    public class BurnStatusData : StatusData
    {
		[field: SerializeField, BoxGroup("Specific")]
		public int Damage { get; private set; }

		[field: SerializeField, BoxGroup("Specific")]
		public int normalDuration { get; private set; } = 2;
    }
}
