using System.Collections.Generic;
using UnityEngine;

namespace Cheats.Core
{
	public abstract class CheatProvider : MonoBehaviour
	{
		public abstract IEnumerable<ICheat> GetCheats();
		
	}
}