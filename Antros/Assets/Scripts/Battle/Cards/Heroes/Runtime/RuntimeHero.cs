using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Phases.Filters;
using ATCG.Capacities;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools.Phases;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Heroes.Runtime
{
    public partial class RuntimeHero : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPhaseListener<SelectCellPhase>
    {

        public static RuntimeHero SelectedHero { get; private set; }

        public event Action OnPointerEnter;
        public event Action OnPointerExit;
        public event Action OnSelected;
        public event Action OnDeselected;

        public event Action OnConnected;
        public event Action OnDisconnected;

        [SerializeField]
        private TMP_Text heroName;

        [SerializeField]
        private SpriteRenderer outline;

        [BoxGroup("Animations")]
        [SerializeField]
        private float hoverScale = 1.15f;

        [BoxGroup("Animations")]
        [SerializeField]
        private float hoverAnimationDuration = .25f;


        [SerializeField, BoxGroup("Animations/Basic Attack")]
        private float windUpDuration;

        [SerializeField, BoxGroup("Animations/Basic Attack")]
        private float windUpScale;

        public bool IsSelected => SelectedHero == this;

        public HeroBattleCard Card { get; private set; }

        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }
        public Condition Selectable { get; private set; }

        private void Awake()
        {
            Selectable = new Condition();
            Selectable.AddOnValueChangeCallback(ctx =>
            {
                if (IsSelected && !ctx)
                    UnSelect();
            });
        }

        public void Initialize(RuntimeBattleGrid battleGrid)
        {
            RuntimeBattleGrid = battleGrid;
        }

        public void Connect(HeroBattleCard card)
        {
            if (Card != null)
                Disconnect();

            Card = card;
            heroName.text = card.Title;
            if (RuntimeBattleGrid.TryGetBattleCellAt(Card.Coordinates, out RuntimeBattleCell cell))
            {
                transform.localScale = cell.GetTargetScale();
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }

            GameplayMetrics metrics = GameplayMetrics.Current;

            Color playerColor =
                metrics.GetPlayerColor(Card.Player.Profile.ID, RuntimeBattleGrid.CurrentBattlePhase.PlayerCount);
            outline.color = playerColor;

            card.RegisterEventRunner(this);

            OnConnected?.Invoke();
        }


        public void Disconnect()
        {
            if (Card == null)
                return;

            Card.UnregisterEventRunner(this);
            OnDisconnected?.Invoke();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!Selectable)
                return;

            Select();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (IsSelected || !Selectable)
                return;

            if (RuntimeBattleGrid.TryGetBattleCellAt(Card.Coordinates, out var cell))
            {
                Tween.StopAll(transform);
                Tween.Scale(transform, cell.GetTargetScale() * hoverScale, hoverAnimationDuration);
            }

        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (IsSelected || !Selectable)
                return;

            if (RuntimeBattleGrid.TryGetBattleCellAt(Card.Coordinates, out var cell))
            {
                Tween.StopAll(transform);
                Tween.Scale(transform, cell.GetTargetScale(), hoverAnimationDuration);
            }
        }

        public async Awaitable DoBasicAttack()
        {
            if (Card.Player is LocalBattlePlayer player && player.canDoBasicAttack)
            {
                UnSelect();
                await Card.PerformBasicAttack();
            }
        }

        public async Awaitable Move()
        {
            if (Card.Player is LocalBattlePlayer player && player.canDoBasicAttack)
            {
                MoveHeroCellFilter filter = new(Card.Speed, Card.Coordinates);

                bool isSelected = IsSelected;
                UnSelect();

                SelectCellPhase phase = new(filter, RuntimeBattleGrid.BattleGrid, player);
                Selectable.AddCondition(phase.SecondaryChannelKey, false);

                PhaseResult<BattleCell> result = await phase.Run();
                if (result is { type: PhaseResultType.Success, value: not null })
                {
                    BattleCell cell = result.value;
                    player.AddOrRemoveMana(-GameplayMetrics.Current.MovementCost);

                    await Card.MoveCard(cell.cell.coordinates);
                }

                Selectable.RemoveCondition(phase.SecondaryChannelKey);
                if (isSelected)
                    Select();
            }
        }

        public void UseCapacity(CapacityData capacityIndex)
        {
            Debug.Log($"Using  capacity {capacityIndex}");
        }

        public void Select()
        {
            if (!Selectable)
                return;

            if (SelectedHero == this)
                return;

            if (SelectedHero != null)
                SelectedHero.UnSelect();

            SelectedHero = this;
            OnSelected?.Invoke();
        }

        public void UnSelect()
        {
            if (SelectedHero == this)
            {
                SelectedHero = null;
                OnDeselected?.Invoke();
            }
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseBegin(SelectCellPhase phase)
        {
            Selectable.AddCondition(phase.MainChannelKey, false);
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseEnd(SelectCellPhase phase)
        {
            Selectable.RemoveCondition(phase.MainChannelKey);
        }
    }
}