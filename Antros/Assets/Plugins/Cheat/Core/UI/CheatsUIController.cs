using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Cheats.Core.UI
{
	public class CheatsUIController : MonoBehaviour
	{
		[SerializeField] private CheatUI cheatUIPrefab;
		[SerializeField] private Transform container;
		[SerializeField] private CheatCollector cheatCollector;
		
		public Dictionary<string, CheatUI> cheats;


		private void Awake()
		{
			cheats = new Dictionary<string, CheatUI>();
		}

		public void ReloadCheats()
		{
			cheatCollector.ScanCheats();

			foreach (var cheat in cheatCollector.GetCheats())
			{
				if (cheats.ContainsKey(cheat.Name))
					return;

				CheatUI instantiate = Instantiate(cheatUIPrefab, container);
				instantiate.SpawnButton(cheat);
				cheats[cheat.Name] = instantiate;
			}
			Debug.Log(cheats.Count);
		}
	}
}