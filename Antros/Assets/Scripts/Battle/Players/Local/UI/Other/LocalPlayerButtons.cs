using ATCG.Battle.Players.Local.Phases;
using ATCG.UI;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Players.Local.UI.Other
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/LocalPlayerButtons")]
    public class LocalPlayerButtons : PlayerHUDElement, IPhaseListener<PlayerTurnPhase>
    {
        [SerializeField]
        private CustomButtonUI endTurnButton;

        [SerializeField]
        private CustomButtonUI giveUpButton;

        public PlayerTurnPhase CurrentPhase { get; private set; }


        private void OnEnable()
        {
            this.Register();
        }


        private void OnDisable()
        {
            this.Unregister();
        }

        void IPhaseListener<PlayerTurnPhase>.OnPhaseBegin(PlayerTurnPhase phase)
        {
            endTurnButton.Interactable = phase.player == Player;
            CurrentPhase = phase;
        }

        void IPhaseListener<PlayerTurnPhase>.OnPhaseEnd(PlayerTurnPhase phase)
        {
            endTurnButton.Interactable = false;
            CurrentPhase = null;
        }

        public void EndTurn(BaseEventData arg0)
        {
            if (CurrentPhase is LocalPlayerTurnPhase localPlayerTurnPhase && localPlayerTurnPhase.player == Player)
                localPlayerTurnPhase.EndTurn();
        }

        public void GiveUp(BaseEventData arg0)
        {
            if (CurrentPhase is LocalPlayerTurnPhase localPlayerTurnPhase && localPlayerTurnPhase.player == Player)
                localPlayerTurnPhase.GiveUp();
        }

        protected override void OnConnect()
        {
        }

        protected override void OnDisconnect()
        {
        }
    }
}