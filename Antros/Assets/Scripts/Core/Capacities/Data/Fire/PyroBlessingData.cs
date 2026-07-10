using System.Collections.Generic;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Status.FireStatus;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities.Data.Fire
{
	[CreateAssetMenu(menuName = "ATCG/Capacities/Fire/PyroBlessing")]
	[WithStep("PyroBlessing")]
	public partial class PyroBlessingData :CapacityData
	{
		[field: SerializeField, BoxGroup("Specific")]
		public PyroFuryData Status {get; private set;}
	}
}