using TMPro;
using UnityEngine;

namespace Cheats.Core.UI
{
	public class CheatUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text Description;
		private ICheat currenCheat;
		public void SpawnButton(ICheat cheat)
		{
			currenCheat = cheat;
			Title.text = cheat.Name;
			Description.text = cheat.Description;
		}

		public void Execute()
		{
			currenCheat.Execute(new CheatContext());
		}
	}
}