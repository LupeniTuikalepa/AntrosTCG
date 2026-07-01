using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;
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
        [SerializeField] private Canvas statPanel;
        private HoverStateUIElement[] panel;

        [Header("Setting")] [SerializeField] private Vector2 offset;

        private void Awake()
        {
            Instance = this;
            statPanel.enabled = false;
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
	        bool canBeSee = false;
	        panel = GetComponentsInChildren<HoverStateUIElement>(true);
	        statPanel.enabled = true;

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
		        statPanel.enabled = false;
	        }
	        
        }

        public void OnPhaseEnd(HoverEntityPhase phase)
        {
	        statPanel.enabled = false;
        }

        private void UiPositon(Vector2 pos)
        {
	        mainPanel.transform.position = pos +  offset;
        }
	}
}
