using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Status.Life
{
	[CreateAssetMenu(menuName = "ATCG/Status/Life/Sustain Status")]
	public class SustainStatusData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")] public float SustainMultiplier {get; private set; } = 1.5f;
		[field: SerializeField, BoxGroup("Specific")] public int Duration {get; private set; } = 3;
	}
}