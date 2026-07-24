using System.Collections.Generic;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Fire
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Fire/EarthQuake")]
	[WithStep("EarthQuake")]
	
	public partial class EarthQuakeData : CapacityData
	{
		[field:SerializeField,BoxGroup("Specific")]
		public EarthQuakeStatusData Status { get;private set; }

		[field: SerializeField, BoxGroup("Custom")]
		public int Range { get; private set; } = 4;

		[field: SerializeField, BoxGroup("Custom")]
		public int Damage { get; private set; } = 3;
	}
}