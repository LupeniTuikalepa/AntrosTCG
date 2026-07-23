using System.Collections.Generic;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Status.FireStatus;
using ATCG.HexGrids.Patterns;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Fire
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Fire/BurningWhip")]
	[WithStep("BurningWhip")]
	public partial class BurningWhipData : CapacityData
	{
		[field:SerializeField,BoxGroup("Specific")]
		public FurnaceData Status { get;private set; }
		
		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; }

		[field: SerializeField, BoxGroup("Custom")]
		public int Damage { get; private set; }
		
		[field: SerializeField, BoxGroup("Custom")]
		public TridentPatternData TridentPatternData { get; private set; }
	}
}