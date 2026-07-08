using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
	[CreateAssetMenu(menuName = "ATCG/Status/Bersek")]
	public class BerserkStatusData : StatusData
	{
		[field: SerializeField] public float forceMultiplier = 1.5f;
		
		[field: SerializeField]public int defenseReduction =2;
		
		[field: SerializeField] public int Duration = 1;
	}
}