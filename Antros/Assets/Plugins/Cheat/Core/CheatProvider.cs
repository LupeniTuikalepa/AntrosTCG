using System.Collections.Generic;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;

namespace Cheats.Core
{
	public abstract class CheatProvider : MonoBehaviour
	{
		public abstract IEnumerable<ICheat> GetCheats();
	}
}