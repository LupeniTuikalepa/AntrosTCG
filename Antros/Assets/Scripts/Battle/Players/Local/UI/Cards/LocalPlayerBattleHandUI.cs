using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Cards.UI;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using ATCG.HexGrids;
using Helteix.Cards;
using Helteix.Cards.Collections;
using Helteix.Cards.UI.Physical;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players.Local.UI.Cards
{
    [AddComponentMenu("ATCG/Gameplay/Cards/Player Hand")]
    public class LocalPlayerBattleHandUI : BattleHandUI, IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
        private PlayerHUD hud;
        protected RuntimeLocalBattlePlayer RuntimeLocalBattlePlayer { get; private set; }
        protected LocalBattlePlayer LocalBattlePlayer => RuntimeLocalBattlePlayer.Player;

        public DeployCardPhase DeployCardPhase { get; private set; }

        private void OnEnable()
        {
            InputUser.onChange += OnInputUserChange;
        }

        private void OnDisable()
        {
            InputUser.onChange -= OnInputUserChange;
        }


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
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer,
            LocalBattlePlayer battlePlayer)
        {
            if (runtimeBattlePlayer == RuntimeLocalBattlePlayer)
            {
                Disconnect();
                RuntimeLocalBattlePlayer = null;
            }
        }

        protected override bool CanCardBeDragged(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeDragged(card);

            return LocalBattlePlayer.canDeployHeroes;
        }

        protected override bool CanCardBeClicked(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeClicked(card);

            return LocalBattlePlayer.canDeployHeroes;
        }

        protected override bool CanCardBeSelected(ICard card)
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
            if (LocalBattlePlayer != null && cardHolderUI.CardUI.Current is IBattleCard gameCard)
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

        protected override void SelectHolder(CardHolderUI holderUI, BaseEventData eventData)
        {
            EventSystem target = EventSystem;
            target.SetSelectedGameObject(holderUI == null ? null : holderUI.gameObject, eventData);
        }

        protected override void OnCardSelect(CardHolderUI holder, BaseEventData eventData)
        {
            holder.transform.localScale = Vector3.one * 1.2f;
            base.OnCardSelect(holder, eventData);
        }

        protected override void OnCardDeselect(CardHolderUI holder, BaseEventData eventData)
        {
            holder.transform.localScale = Vector3.one;
            base.OnCardDeselect(holder, eventData);
        }

        private void SelectFirstAvailableHolder()
        {
            if (TryGetHolderFor(LocalBattlePlayer.Hand.GetCard(0), out CardHolderUI holder))
                SelectHolder(holder, null);
        }

        private void OnInputUserChange(InputUser inputUser, InputUserChange change, InputDevice device)
        {
            if (RuntimeLocalBattlePlayer == null)
                return;

            if (inputUser != RuntimeLocalBattlePlayer.Controls.PlayerInputUser)
                return;

            switch (change)
            {
                case InputUserChange.ControlSchemeChanged:
                    if (inputUser.controlScheme is { name: "Gamepad" })
                        SelectFirstAvailableHolder();
                    break;
            }
        }

        protected override void OnCardBeginDrag(CardHolderUI holder, PointerEventData eventData)
        {
            base.OnCardBeginDrag(holder, eventData);
            if (LocalBattlePlayer.canDeployHeroes && DeployCardPhase == null && holder.CardUI.Current is IBattleCard card)
                _ = DeployPlayerCard(card);
        }

        private async Awaitable DeployPlayerCard(IBattleCard card)
        {
            try
            {
                int id = LocalBattlePlayer.Hand.GetCardIndex(card);
                if(id == -1)
                    return;

                DeployCardPhase = new DeployCardPhase(LocalBattlePlayer, LocalBattlePlayer.BattlePhase.BattleGrid, card);
                PhaseResult<HexCoordinates> phaseResult = await DeployCardPhase.Run();


                if (phaseResult is { type: PhaseResultType.Success, value: { IsValid: true } })
                {
                    DeployCardCommand deployCardCommand = new DeployCardCommand(id, phaseResult.value, LocalBattlePlayer.ID);

                    await GameCommandManager.Instance.ExecuteGameCommand(deployCardCommand, LocalBattlePlayer.BattlePhase);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                DeployCardPhase = null;
            }
        }
    }
}