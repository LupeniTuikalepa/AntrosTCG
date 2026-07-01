using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities;
using Helteix.Cards;
using Helteix.Tools;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class CapacitiInfoStat : HoverStateUIElement
	{
		[SerializeField] private TMP_Text nameText;
		
		
		[SerializeField] private GameObject capaPanel;
		
		[SerializeField] private Transform capaContainer;
		[SerializeField] private GameObject capaPrefab;
		[SerializeField] private List<SetupCapaPanel> cards;
		
		public override bool Build()
		{
			ClearCapacities();
			
			if (capaPrefab == null)
			{
				Debug.LogError($"[InfoStateUIPanel] Le préfab 'capaPrefab' n'est pas assigné dans l'inspecteur de {gameObject.name} !", this);
				return false;
			}
			if (EntityPhase.HoveredAddress.TryGetComponentRO(out BattleCardComponent battleCard))
			{
				capaPanel.SetActive(true);
				nameText.text = battleCard.battleCard.Title;
				if (battleCard.battleCard.CapacitiesData == null) return true;
				foreach (var capacityData in battleCard.battleCard.CapacitiesData)
				{
					if (capacityData == null) continue;
					GameObject capaUi = Instantiate(capaPrefab, capaContainer);
					var capaPanel = capaUi.GetComponent<SetupCapaPanel>();
					if (capaPanel != null)
					{
						capaPanel.SetupCapa(capacityData);
						cards.Add(capaPanel);
					}
				}
				return true;
			}
			capaPanel.SetActive(false);
			return false;
		}
		
		private void ClearCapacities()
		{
			foreach (Transform child in capaContainer)
			{
				Destroy(child.gameObject);
			}
			foreach (SetupCapaPanel capaPanel in cards)
			{
				if (capaPanel != null) Destroy(capaPanel.gameObject);
			}
			cards.Clear();
		}
		
		private void OnDisable()
		{
			ClearCapacities();
		}
	}
}