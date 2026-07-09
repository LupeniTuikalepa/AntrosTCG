using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
	[CreateAssetMenu(menuName = "ATCG/Status/Bersek")]
	public class BerserkStatusData : StatusData
	{
		[field: SerializeField] public float forceMultiplier {get; private set; } = 1.5f;
		
		[field: SerializeField]public int defenseReduction {get; private set; } =2;
	}
}