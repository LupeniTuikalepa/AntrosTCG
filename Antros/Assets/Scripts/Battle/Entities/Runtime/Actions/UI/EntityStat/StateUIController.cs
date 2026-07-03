using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.Tools.Phases;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
	public class StateUIController: RuntimeLocalPlayerComponent,IPhaseListener<HoverEntityPhase>
	{
        public static StateUIController Instance { get; private set; }

        [Header("UI Components")] [SerializeField]
        private Transform mainPanel;
        [SerializeField] private GameObject statPanel;
        private HoverStateUIElement[] panel;
        

        [Header("Setting")] 
        [SerializeField] private Vector2 offset;
        
        private Transform hoveredEntityTransform;

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
	        if (RuntimeEntityManager.TryGetRuntimeEntity(phase.HoveredAddress, out IRuntimeEntity runtimeEntity))
	        {
		        UiPositon(runtimeEntity.HoveredRoot.position);
	        }
	        panel = GetComponentsInChildren<HoverStateUIElement>(true);
	        statPanel.SetActive(true);

	        foreach (HoverStateUIElement uiElement in panel)
	        {
		        uiElement.Connect(phase);
		        if (uiElement.isActiveAndEnabled)
		        {
			        canBeSee = true;
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

        private void UiPositon(Vector3 pos)
        {
	        Vector2 position = RuntimeLocalBattlePlayer.Camera.Component.OutputCamera.WorldToScreenPoint(pos);
	        mainPanel.transform.position = position + offset;
        }

        protected override void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
	        
        }
        protected override void Disconnect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
        }
	}
}
