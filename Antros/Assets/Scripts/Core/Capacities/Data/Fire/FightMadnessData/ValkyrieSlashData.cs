using ATCG.Capacities;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG
{
	[CreateAssetMenu (menuName = "ATCG/Capacities/Fire/ValkyrieSlash")]
	[WithStep("Slash")]
	public partial class ValkyrieSlashData : CapacityData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public int Damage { get;private set; } = 2;

		[field: SerializeField, BoxGroup("Specific")]
		public int Range { get;private set; } = 1;

		[field: SerializeField, BoxGroup("Specific")]
		public int BerserkDamage { get; private set; } = 4;

		[field: SerializeField, BoxGroup("Specific")]
		public int BerserkRange { get; private set; } = 2;

		[field: SerializeField, BoxGroup("Specific")]
		public int EnnemyQuantitiesApplyStatus { get; set; }

		[field: SerializeField, BoxGroup("Specific")]
		public BerserkStatusData status;
	}
}