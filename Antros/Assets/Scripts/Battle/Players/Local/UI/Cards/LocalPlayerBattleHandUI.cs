using ATCG.Battle.Cards;
using ATCG.Battle.Cards.UI;
using ATCG.Battle.Players.Runtime;
using ATCG.Battle.Players.Runtime.UI;
using Helteix.Cards;
using Helteix.Cards.Collections;
using Helteix.Cards.UI.Physical;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players.Local.UI.Cards
{
    [AddComponentMenu("ATCG/Gameplay/Cards/Player Hand")]
    public class LocalPlayerBattleHandUI : BattleHandUI, IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
        protected RuntimeLocalBattlePlayer RuntimeLocalBattlePlayer { get; private set; }
        protected LocalBattlePlayer LocalBattlePlayer => RuntimeLocalBattlePlayer.Player;

        [SerializeField]
        private EventSystem eventSystem;

        private PlayerHUD hud;


        private void OnEnable()
        {
            InputUser.onChange += OnInputUserChange;
        }

        private void OnDisable()
        {
            InputUser.onChange -= OnInputUserChange;
        }

        protected override bool CanCardBeDragged(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeDragged(card);

            return true;
            return LocalBattlePlayer.canDeployHeroes;
        }

        protected override bool CanCardBeClicked(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeClicked(card);

            return true;
            return LocalBattlePlayer.canDeployHeroes;
        }

        protected override bool CanCardBeSelected(ICard card)
        {
            if (LocalBattlePlayer == null)
                return base.CanCardBeSelected(card);

            return true;
            return LocalBattlePlayer.canDeployHeroes;
        }

        protected override bool TryGetCardAtDirection(CardHolderUI cardHolderUI, Vector2 moveVector, out CardHolderUI holderUI)
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
            EventSystem target = eventSystem == null ? EventSystem.current : eventSystem;
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


        private void OnControlsChanged(PlayerInput playerInput)
        {

        }

        [Button]
        private void SelectFirstAvailableHolder()
        {
            if(TryGetHolderFor(LocalBattlePlayer.Hand.GetCard(0), out var holder))
                SelectHolder(holder, null);
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            if (runtimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
            {
                if (RuntimeLocalBattlePlayer != null)
                    Disconnect();

                RuntimeLocalBattlePlayer = runtimeLocalBattlePlayer;
                Connect(LocalBattlePlayer.Hand);
            }
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer battlePlayer)
        {
            if (runtimeBattlePlayer == RuntimeLocalBattlePlayer)
            {
                Disconnect();
                RuntimeLocalBattlePlayer = null;
            }
        }

        private void OnInputUserChange(InputUser inputUser, InputUserChange change, InputDevice device)
        {
            if(RuntimeLocalBattlePlayer == null || RuntimeLocalBattlePlayer.Player == null)
                return;

            if(inputUser.index != RuntimeLocalBattlePlayer.Player.Profile.InputUser.index)
                return;

            switch (change)
            {
                case InputUserChange.ControlSchemeChanged:
                    if (device is Gamepad)
                        SelectFirstAvailableHolder();
                    break;
            }
        }

    }
}