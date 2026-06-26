using System;
using System.Collections.Generic;
using ATCG.Utilities;
using Helteix.Tools.Phases.Listeners;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Debugging.Debugging.Battle.ChoicePhase
{
	public class CheatChoicePhaseUI : MonoPhaseListener<CheatsChoicePhase>
	{
		private CheatsChoicePhase current;
		[SerializeField]
		private CanvasGroup canvasGroup;
		
		[SerializeField]
		private TMP_Dropdown dropdown;

		private void Awake()
		{
			canvasGroup.Hide(0);
		}
		protected override void OnPhaseBegin(CheatsChoicePhase phase)
		{
			current = phase;
			dropdown.ClearOptions();
			canvasGroup.Show(0.3f);

			List<string> options = current.choices;
			dropdown.AddOptions(options);
			dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
			
			base.OnPhaseBegin(phase);
		}

		protected override void OnPhaseEnd(CheatsChoicePhase phase)
		{
			canvasGroup.Hide(0.2f);
			base.OnPhaseEnd(phase);
		}

		public void Cancel()
		{
			current.SetResult(string.Empty);
		}

		public void OnDropdownValueChanged(int index)
		{
			string stringChoisie = dropdown.options[index].text;
			Debug.Log("Tu as cliqué sur : " + stringChoisie);
			current.SetResult(stringChoisie);
		}
	}
}