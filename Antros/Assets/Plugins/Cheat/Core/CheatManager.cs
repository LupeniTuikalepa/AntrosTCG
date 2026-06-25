using System.Collections.Generic;
using UnityEngine;

namespace Cheats.Core
{
	public static class CheatManager
	{
		private static List<ICheat>  cheats = new List<ICheat>();

		[RuntimeInitializeOnLoadMethod]
		private static void InitializeCheats()
		{
			cheats.Clear();
		}
		
		public static IEnumerable<ICheat> GetCheats()
		{
			return cheats;
		}
		
		public static void ScanCheats()
		{
			
			CheatProvider[] providers = GameObject.FindObjectsByType<CheatProvider>();
			for (int i = 0; i < providers.Length; i++)
			{
				
				CheatProvider provider = providers[i];
				cheats.AddRange(provider.GetCheats());
			}
		}
	}
}