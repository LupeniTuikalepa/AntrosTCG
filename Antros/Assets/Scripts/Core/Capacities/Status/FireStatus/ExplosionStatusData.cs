using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Status.FireStatus
{
	[CreateAssetMenu(menuName = "ATCG/Status/Fire/ExplosionData")]
	public class ExplosionData : StatusData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int MainDamage { get; private set; } = 5;

		[field: SerializeField, BoxGroup("Specific")]
		public int SecondeDamage { get; private set; } = 3;

		[field: SerializeField, BoxGroup("Specific")]
		public int Duration { get; private set; } = 2;

		[field: SerializeField, BoxGroup("Specific")]
		public int Range { get; set; } = 1;
	}
}