using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
	[CreateAssetMenu(menuName = "ATCG/Status/Bersek")]
	public class BerserkStatusData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")] public float forceMultiplier {get; private set; } = 1.5f;
		
		[field: SerializeField, BoxGroup("Specific")]public int defenseReduction {get; private set; } =2;
	}
}