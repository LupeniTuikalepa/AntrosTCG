using System;
using ATCG.Battle.Players.Local.Phases;
using ATCG.UI;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCG.Battle.Players.Runtime.UI.Other
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/LocalPlayerButtons")]
    public class LocalPlayerButtons : PlayerHUDElement, IPhaseListener<LocalPlayerTurnPhase>
    {
        [SerializeField]
        private CustomButtonUI endTurnButton;
        [SerializeField]
        private CustomButtonUI giveUpButton;
        public LocalPlayerTurnPhase CurrentPhase { get; private set; }

        private void Awake()
        {
            endTurnButton.OnClick.AddListener(EndTurn);
            giveUpButton.OnClick.AddListener(GiveUp);
        }

        private void OnEnable()
        {
            this.Register();
        }


        private void OnDisable()
        {
            this.Unregister();
        }

        public void EndTurn(BaseEventData arg0) => CurrentPhase?.EndTurn();
        private void GiveUp(BaseEventData arg0) => CurrentPhase?.GiveUp();

        protected override void OnConnect()
        {

        }

        protected override void OnDisconnect()
        {

        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseBegin(LocalPlayerTurnPhase phase)
        {
            endTurnButton.Interactable = phase.localPlayerTurn == Player;
            CurrentPhase = phase;
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseEnd(LocalPlayerTurnPhase phase)
        {
            endTurnButton.Interactable = false;
        }
    }
}