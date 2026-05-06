using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using ATCG.Metrics;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public partial class RuntimeHero : RuntimeEntity<HeroEntityAspect>
    {
        [SerializeField]
        private TMP_Text heroName;

        [SerializeField]
        private SpriteRenderer outline;

        [BoxGroup("Animations"), SerializeField, Range(0, 1)]
        private float baseScale = .85f;

        [SerializeField]
        private float hoverScale = 1.15f;

        [BoxGroup("Animations"), SerializeField]
        private float hoverAnimationDuration = .25f;


        [SerializeField, BoxGroup("Animations/Basic Attack")]
        private float windUpDuration;

        [SerializeField, BoxGroup("Animations/Basic Attack")]
        private float windUpScale;

        [SerializeField, BoxGroup("GameFeel"), Range(0, 30)]
        private float movementDuration;


        public RuntimeBattleGrid RuntimeBattleGrid => Manager.RuntimeBattleGrid;

        public HeroEntityAspect Hero { get; private set; }


        protected override void OnPointerEnter(PointerEventData pointerEventData)
        {
            if (IsSelected || !Manager.Selectable)
                return;

            Tween.StopAll(transform);
            Tween.Scale(transform, GetHeroScale() * hoverScale, hoverAnimationDuration);
        }

        protected override void OnPointerExit(PointerEventData pointerEventData)
        {
            base.OnPointerEnter(pointerEventData);

            Tween.StopAll(transform);
            Tween.Scale(transform, GetHeroScale(), hoverAnimationDuration);
        }


        public override void Connect(RuntimeEntityManager manager, HeroEntityAspect aspect)
        {
            Hero = aspect;

            heroName.text = aspect.Name;
            if (RuntimeBattleGrid.TryGetBattleCellAt(aspect.BattleGridElementComponent.coordinates, out RuntimeBattleCell cell))
            {
                transform.localScale = GetHeroScale();
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }

            GameMetrics metrics = GameMetrics.Current;

            int playerCount = RuntimeBattleGrid.CurrentBattlePhase.PlayerCount;
            Color playerColor = metrics.GetPlayerColor(aspect.Player.GetPlayerID(), playerCount);
            outline.color = playerColor;
        }

        private Vector3 GetHeroScale()
        {
            return RuntimeBattleGrid.GetTargetScale() * baseScale;
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
    }
}