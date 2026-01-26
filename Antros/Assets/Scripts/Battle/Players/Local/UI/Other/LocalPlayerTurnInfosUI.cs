using System;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime.UI.Other
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

        protected override void OnConnect()
        {

        }

        protected override void OnDisconnect()
        {

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
    }
}