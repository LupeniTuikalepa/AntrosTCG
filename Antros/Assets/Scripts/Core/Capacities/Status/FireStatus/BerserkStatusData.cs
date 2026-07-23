using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status
{
	[CreateAssetMenu(menuName = "ATCG/Status/Bersek")]
	public class BerserkStatusData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")] public float ForceMultiplier {get; private set; } = 1.5f;
		
		[field: SerializeField, BoxGroup("Specific")]public int DefenseReduction {get; private set; } =2;
	}
}