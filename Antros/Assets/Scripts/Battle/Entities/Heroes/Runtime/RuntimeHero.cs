using System;
using System.Threading.Tasks;
using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Entities.Heroes;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using ATCG.HexGrids;
using ATCG.Metrics;
using Helteix.Tools.Phases;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace ATCG.Battle.Heroes.Runtime
{
    public partial class RuntimeHero : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        public event Action OnHeroSelected;
        public event Action OnHeroDeselected;

        public event Action OnConnected;
        public event Action OnDisconnected;

        [SerializeField]
        private TMP_Text heroName;

        [SerializeField]
        private SpriteRenderer outline;

        [BoxGroup("Animations")]
        [SerializeField, Range(0, 1)]
        private float baseScale = .85f;

        [SerializeField]
        private float hoverScale = 1.15f;

        [BoxGroup("Animations")]
        [SerializeField]
        private float hoverAnimationDuration = .25f;


        [SerializeField, BoxGroup("Animations/Basic Attack")]
        private float windUpDuration;

        [SerializeField, BoxGroup("Animations/Basic Attack")]
        private float windUpScale;

        [SerializeField, BoxGroup("GameFeel"), Range(0, 30)]
        private float movementDuration;

        public HeroEntity Hero { get; private set; }
        public RuntimeHeroManager Manager { get; private set; }

        public RuntimeBattleGrid RuntimeBattleGrid => Manager.RuntimeBattleGrid;
        public bool IsSelected => Manager.SelectedCard == this;


        public void Initialize(RuntimeHeroManager battleGrid)
        {
            Manager = battleGrid;
        }

        public void Connect(HeroEntity hero)
        {
            if (Hero != null)
                Disconnect();

            Hero = hero;
            heroName.text = hero.Name;
            if (RuntimeBattleGrid.TryGetBattleCellAt(Hero.Coordinates, out RuntimeBattleCell cell))
            {
                transform.localScale = GetHeroScale();
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }

            GameMetrics metrics = GameMetrics.Current;

            Color playerColor =
                metrics.GetPlayerColor(Hero.Player.Profile.ID, RuntimeBattleGrid.CurrentBattlePhase.PlayerCount);
            outline.color = playerColor;

            //hero.RegisterEventRunner(this);

            OnConnected?.Invoke();
        }

        private Vector3 GetHeroScale() => RuntimeBattleGrid.GetTargetScale() * baseScale;

        public void Disconnect()
        {
            if (Hero == null)
                return;

            //Hero.UnregisterEventRunner(this);
            OnDisconnected?.Invoke();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (selectCellPhases.Count > 0)
            {
                using (ListPool<SelectCellPhase>.Get(out var list))
                {
                    list.AddRange(selectCellPhases);
                    foreach (SelectCellPhase phase in list)
                    {
                        if (phase.IsCoordValid(Hero.Coordinates))
                            phase.SetResult(Hero.Coordinates);
                    }
                }
            }
            if (!Manager.Selectable)
                return;

            Select();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (IsSelected || !Manager.Selectable)
                return;

            if (RuntimeBattleGrid.TryGetBattleCellAt(Hero.Coordinates, out var cell))
            {
                Tween.StopAll(transform);
                Tween.Scale(transform, GetHeroScale() * hoverScale, hoverAnimationDuration);
            }

        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (IsSelected || !Manager.Selectable)
                return;

            if (RuntimeBattleGrid.TryGetBattleCellAt(Hero.Coordinates, out var cell))
            {
                Tween.StopAll(transform);
                Tween.Scale(transform, GetHeroScale(), hoverAnimationDuration);
            }
        }

        public async Awaitable DoBasicAttack()
        {
            await Task.CompletedTask;
            /*
            if (Hero.Player is LocalBattlePlayer player && player.canDoBasicAttack)
            {
                UnSelect();
                await Hero.PerformBasicAttack();
            }
            */
        }

        public async Awaitable Move()
        {
            await Task.CompletedTask;
            /*
            if (Hero.Player is LocalBattlePlayer player && player.canDoBasicAttack)
            {
                using (ListPool<HexCoordinates>.Get(out var list))
                {
                    Hero.GetMovableCoords(list);
                    SelectCellPhase phase = new(list, RuntimeBattleGrid.BattleGrid, player);

                    PhaseResult<BattleCell> result = await phase.Run();

                    if (result is { type: PhaseResultType.Success, value: not null })
                    {
                        BattleCell cell = result.value;
                        player.AddOrRemoveMana(-GameMetrics.Current.MovementCost);

                        await Hero.MoveCard(cell.cell.coordinates);
                    }
                }
            }
            */
        }

        public void UseCapacity(CapacityData capacityIndex)
        {
            Debug.Log($"Using  capacity {capacityIndex}");
        }

        public void Select()
        {
            Manager.Select(this);
        }

        public void UnSelect()
        {
            if(Manager.SelectedCard == this)
                Manager.Unselect();
        }

        public void OnSelected()
        {
            Tween.StopAll(transform);
            transform.localScale = GetHeroScale();

            Tween.PunchScale(transform, Vector3.one * -5, .25f);
            OnHeroSelected?.Invoke();
        }

        public void OnDeselected()
        {
            Tween.StopAll(transform);
            transform.localScale = GetHeroScale();

            OnHeroDeselected?.Invoke();
        }
    }
}