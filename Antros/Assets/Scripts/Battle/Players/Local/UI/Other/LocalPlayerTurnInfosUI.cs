using Helteix.Tools.Phases;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Players.Local.UI.Other
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/LocalPlayerTurnInfosUI")]
    public class LocalPlayerTurnInfosUI : PlayerHUDElement, IPhaseListener<PlayerTurnPhase>
    {
        [SerializeField]
        private TMP_Text playerName;

        [SerializeField]
        private TMP_Text turnNumber;

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        public void OnPhaseBegin(PlayerTurnPhase phase)
        {
            if (phase.player == Player)
                playerName.text = "Your turn";
            else
                playerName.text = $"{phase.player.Profile.Infos.name}'s turn";

            turnNumber.text = $"Turn {phase.turnNumber}";
        }

        public void OnPhaseEnd(PlayerTurnPhase phase)
        {
        }

        protected override void OnConnect()
        {
        }

        protected override void OnDisconnect()
        {
        }
    }
}