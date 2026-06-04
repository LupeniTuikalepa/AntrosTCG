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
using Helteix.Tools;
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

        [BoxGroup("Animations"), SerializeField, Range(0, 1)]
        private float baseScale = .85f;

        [SerializeField, BoxGroup("Animations/Basic Attack")]
        private float windUpDuration;

        [SerializeField, BoxGroup("Animations/Basic Attack")]
        private float windUpScale;

        [SerializeField, BoxGroup("GameFeel"), Range(0, 30)]
        private float movementDuration;


        public RuntimeBattleGrid RuntimeBattleGrid => Manager.RuntimeBattleGrid;

        public HeroEntityAspect Hero { get; private set; }



        public override async Awaitable Spawn(RuntimeEntityManager manager, HeroEntityAspect aspect)
        {
            await base.Spawn(manager, aspect);

            Debug.Log($"[Runtime Hero] {aspect.BattleCardComponent.battleCard.Title} spawned.");
            Hero = aspect;
            heroName.text = aspect.Name;

            manager.RegisterRuntimeEntity(this);

            if (RuntimeBattleGrid.TryGetBattleCellAt(aspect.BattleGridElementComponent.coordinates, out RuntimeBattleCell cell))
            {
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                await Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }

            GameMetrics metrics = GameMetrics.Current;

            int playerCount = RuntimeBattleGrid.CurrentBattlePhase.PlayerCount;
            int playerID = aspect.Player.GetPlayerID();

            RenderingLayerMask mask = RenderingLayerMask.GetMask($"Player{playerID + 1}");
            if(mask.value != 0)
                Model.EnableRenderingLayer(mask);
        }

        public void Despawn(RuntimeEntityManager manager)
        {
            manager.UnregisterRuntimeEntity(this);
        }


        public async Awaitable DoBasicAttack()
        {
            await Task.CompletedTask;
        }

        public async Awaitable Move()
        {
            await Task.CompletedTask;
        }

        public void UseCapacity(CapacityData capacityIndex)
        {
            Debug.Log($"Using  capacity {capacityIndex}");
        }
    }
}