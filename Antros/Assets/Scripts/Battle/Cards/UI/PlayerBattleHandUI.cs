using System;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using Helteix.Cards;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Cards.UI
{
    [AddComponentMenu("ATCG/Gameplay/Cards/Player Hand")]
    public class PlayerBattleHandUI : BattleHandUI, IPhaseListener<LocalPlayerTurnPhase>
    {
        public LocalBattlePlayer LocalBattlePlayer { get; private set; }

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        protected override bool CanCardBeDragged(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeDragged(card);

            return LocalBattlePlayer.canDeployHeroes;
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseBegin(LocalPlayerTurnPhase phase)
        {
            if(LocalBattlePlayer != null)
                Disconnect();

            LocalBattlePlayer = phase.battlePlayer;
            Connect(LocalBattlePlayer.Hand);
        }

        void IPhaseListener<LocalPlayerTurnPhase>.OnPhaseEnd(LocalPlayerTurnPhase phase)
        {
            if (phase.battlePlayer == LocalBattlePlayer)
            {
                Disconnect();
            }
        }

    }
}