using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Cheats.Core.UI
{
	public class CheatsUIController : MonoBehaviour
	{
		[SerializeField] private GameObject menuCheatPrefab;
		[SerializeField] private CheatUI cheatUIPrefab;
		[SerializeField] private Transform container;
		public Dictionary<ICheat, CheatUI> cheats;


		private void Awake()
		{
			cheats = new Dictionary<ICheat, CheatUI>();
		}

		public void ReloadCheats()
		{
			cheats.Clear();
			Debug.Log(cheats.Count);
			CheatManager.ScanCheats();

			IEnumerable<ICheat> cheatList = CheatManager.GetCheats();

			foreach (var cheat in cheatList)
			{
				if (cheats.ContainsKey(cheat))
					return;

				CheatUI instantiate = Instantiate(cheatUIPrefab, container);
				instantiate.SpawnButton(cheat);
				cheats[cheat] = instantiate;
			}
		}
	}
}