using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Frost
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Frost/Shell")]
	[WithStep("Shell")]
	public partial class ShellData : CapacityData
	{
		[field:SerializeField,BoxGroup("Specific")]
		public HardeningData Status { get;private set; }

		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; } = 1;
	}
}