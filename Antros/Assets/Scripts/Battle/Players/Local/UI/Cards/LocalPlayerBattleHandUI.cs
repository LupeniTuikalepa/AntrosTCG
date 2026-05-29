using System;
using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Cards.UI;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using ATCG.HexGrids;
using ATCG.Utilities;
using Helteix.Cards;
using Helteix.Cards.Collections;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.UI.Cards
{
    [AddComponentMenu("ATCG/Gameplay/Cards/Player Hand")]
    public class LocalPlayerBattleHandUI : BattleHandUI, IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
        private PlayerHUD hud;
        protected RuntimeLocalBattlePlayer RuntimeLocalBattlePlayer { get; private set; }
        protected LocalBattlePlayer LocalBattlePlayer => RuntimeLocalBattlePlayer.Player;

        public SelectCellPhase DeployCard { get; private set; }


        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer,
            LocalBattlePlayer player)
        {
            if (runtimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
            {
                if (RuntimeLocalBattlePlayer != null)
                    Disconnect();

                RuntimeLocalBattlePlayer = runtimeLocalBattlePlayer;
                Connect(LocalBattlePlayer.Hand);
            }

            (this as IRuntimeBattlePlayerComponent<IBattlePlayer>).Connect(runtimeBattlePlayer, player);
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer,
            LocalBattlePlayer player)
        {
            if (runtimeBattlePlayer == RuntimeLocalBattlePlayer)
            {
                Disconnect();
                RuntimeLocalBattlePlayer = null;
            }

            (this as IRuntimeBattlePlayerComponent<IBattlePlayer>).Disconnect(runtimeBattlePlayer, player);
        }

        public override bool CanCardBeDragged(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeDragged(card);

            return LocalBattlePlayer.canDeployHeroes;
        }

        public override bool CanCardBeClicked(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeClicked(card);

            return LocalBattlePlayer.canDeployHeroes;
        }

        public override bool CanCardBeSelected(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeSelected(card);

            return true;
        }

        public override bool CanCardBeSubmitted(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeSelected(card);

            return LocalBattlePlayer.canDeployHeroes;
        }

        protected override bool TryGetCardAtDirection(CardHolderUI cardHolderUI, Vector2 moveVector,
            out CardHolderUI holderUI)
        {
            if (LocalBattlePlayer != null && cardHolderUI.CardUI.Card is IBattleCard gameCard)
            {
                Hand<IBattleCard> hand = LocalBattlePlayer.Hand;
                int index = hand.GetCardIndex(gameCard);

                if (index > 0)
                {
                    int horizontalDir = (int)Mathf.Sign(moveVector.x);
                    int targetIndex = index + horizontalDir;
                    if (targetIndex < 0)
                        targetIndex = LocalBattlePlayer.Hand.CurrentSize - 1;

                    if (targetIndex >= LocalBattlePlayer.Hand.CurrentSize)
                        targetIndex = 0;

                    IBattleCard next = hand.GetCard(targetIndex);
                    return TryGetHolderFor(next, out holderUI);
                }
            }

            return base.TryGetCardAtDirection(cardHolderUI, moveVector, out holderUI);
        }



        public override bool BeginCardDrag(CardHolderUI holderUI)
        {
            if (holderUI.CardUI.Card is not IBattleCard card)
                return false;

            if (LocalBattlePlayer.canDeployHeroes && DeployCard == null)
                return false;

            if (base.BeginCardDrag(holderUI) && TryGetDragPhase(card, out CardDragPhase<IBattleCard> phase))
            {
                _ = DoCardDeploy(card, phase);
                return true;
            }

            return false;
        }

        public override bool OnCardDrop(CardHolderUI holderUI, DragResult<IBattleCard> result)
        {
            if (base.OnCardDrop(holderUI, result) && DeployCard != null)
            {
                if (result.Target is RuntimeBattleCell runtimeBattleCell)
                {
                    DeployCard.SetResult(runtimeBattleCell.Address);
                    return true;
                }

                DeployCard.Cancel();
            }
            return false;
        }

        private async Awaitable DoCardDeploy(IBattleCard card, CardDragPhase<IBattleCard> phase)
        {
            if (DeployCard != null)
                return;

            using (ListPool<HexCoordinates>.Get(out List<HexCoordinates> cells))
            {
                BattleGrid battleGrid = LocalBattlePlayer.BattlePhase.BattleGrid;
                battleGrid.FillDeployableCells(cells);

                DeployCard = new SelectCellPhase(cells, battleGrid, LocalBattlePlayer);

                var result = await DeployCard.Run();
                DeployCard = null;
                if (result.type != PhaseResultType.Success)
                    return;

                int cardID = LocalBattlePlayer.Hand.GetCardIndex(card);
                if (cardID != -1 && result.value.IsBattleCellAspect(out BattleCellAspect aspect))
                {
                    DeployCardCommand deployCardCommand = new DeployCardCommand(cardID, aspect.Coordinate, LocalBattlePlayer.ID);
                    deployCardCommand.RunAndForget(Player.BattlePhase);
                }
            }
        }
    }
}