using System;
using System.Collections.Generic;
using ATCG;
using ATCG.Capacities;
using ATCG.Capacities.UI;
using ATCG.Elements;
using CopyCapa;
using Helteix.Tools;
using StealCapa.UI;
using UnityEngine;

namespace StealCapa
{
	public class GetAllCapa : MonoBehaviour
	{
		[SerializeField] private GameObject panelUI;
		[SerializeField] private CapacityUI capaUIPrefab;
		[SerializeField] private Transform container;

		private void Start()
		{
			panelUI.SetActive(false);
			container.ClearChildren();
		}

		public void PopulatePanel(CopyCapaPhase phase)
		{
			container.ClearChildren();
          
			foreach (CapacityData capacityData in GameController.GameDatabase.GetAll<CapacityData>())
			{
				var clone = Instantiate(capaUIPrefab, container);
				clone.Connect(capacityData);
				
				if (clone.TryGetComponent<CapaSelected>(out var selector))
				{
					selector.Initialize(phase);
				}
			}
		}
	}
}
