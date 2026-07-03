using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class StateUIController: MonoBehaviour,IPhaseListener<HoverEntityPhase>
	{
        public static StateUIController Instance { get; private set; }

        [Header("UI Components")] [SerializeField]
        private Transform mainPanel;
        [SerializeField] private GameObject statPanel;
        private HoverStateUIElement[] panel;

        [Header("Setting")] [SerializeField] private Vector2 offset;

        private bool canBeSee;
        private void Awake()
        {
            Instance = this;
            statPanel.SetActive(false);
        }
        
        private void OnEnable()
        {
	        this.Register();
        }

        private void OnDisable()
        {
	        this.Unregister();
        }

        public void OnPhaseBegin(HoverEntityPhase phase)
        {
	        canBeSee = false;
	        panel = GetComponentsInChildren<HoverStateUIElement>(true);
	        statPanel.SetActive(true);

	        foreach (HoverStateUIElement uiElement in panel)
	        {
		        uiElement.Connect(phase);
		        if (uiElement.isActiveAndEnabled)
		        {
			        canBeSee = true;
			        
			        UiPositon(Mouse.current.position.ReadValue());
		        }
	        }

	        if (!canBeSee)
	        {
		        statPanel.SetActive(false);
	        }
	        
        }

        public void OnPhaseEnd(HoverEntityPhase phase)
        {
	        statPanel.SetActive(false);
        }

        private void UiPositon(Vector2 pos)
        {
	        mainPanel.transform.position = pos +  offset;
        }
	}
}
