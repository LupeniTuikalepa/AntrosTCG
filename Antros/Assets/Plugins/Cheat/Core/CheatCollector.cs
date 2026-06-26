using System.Collections.Generic;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;

namespace Cheats.Core
{
	public class CheatCollector : MonoBehaviour
	{
		private List<ICheat> cheats = new List<ICheat>();
		[SerializeField] private RuntimeLocalBattlePlayer players;

		public IEnumerable<ICheat> GetCheats()
		{
			return cheats;
		}

		public void ScanCheats()
		{
			cheats.Clear();

			CheatProvider[] providers = players.GetComponentsInChildren<CheatProvider>();
			
			for (int i = 0; i < providers.Length; i++)
			{
				CheatProvider provider = providers[i];
				cheats.AddRange(provider.GetCheats());
			}
		}
	}
}