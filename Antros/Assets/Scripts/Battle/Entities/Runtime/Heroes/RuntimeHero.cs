using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Capacities;
using ATCG.Metrics;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public partial class RuntimeHero : RuntimeEntity<HeroEntityAspect>, IPhaseListener<SelectEntityActionPhase>
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

        [SerializeField] private CinemachineCamera cinemachineCamera;

        public RuntimeBattleGrid RuntimeBattleGrid => Manager.RuntimeBattleGrid;


        protected override void OnEnable()
        {
            PhaseManager.Register<SelectEntityActionPhase>(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            PhaseManager.Unregister<SelectEntityActionPhase>(this);
            base.OnDisable();
        }


        public override async Awaitable Spawn(RuntimeEntityManager manager, HeroEntityAspect aspect)
        {
            await base.Spawn(manager, aspect);

            Debug.Log($"[Runtime Hero] {aspect.BattleCardComponent.battleCard.Title} spawned.");

            heroName.text = aspect.Name;

            manager.RegisterRuntimeEntity(this);

            if (RuntimeBattleGrid.TryGetBattleCellAt(aspect.GridMemberComponent.coordinates, out RuntimeBattleCell cell))
            {
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                await Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }

            GameMetrics metrics = GameMetrics.Current;

            int playerID = aspect.Player.GetPlayerID();

            RenderingLayerMask mask = RenderingLayerMask.GetMask($"Player{playerID + 1}");
            if(mask.value != 0)
                Model.EnableRenderingLayer(mask);
        }

        public void Despawn(RuntimeEntityManager manager)
        {
            manager.UnregisterRuntimeEntity(this);
        }


        void IPhaseListener<SelectEntityActionPhase>.OnPhaseBegin(SelectEntityActionPhase phase)
        {
            if (IsSelected)
            {
                cinemachineCamera.gameObject.SetActive(true);
            }
        }

        void IPhaseListener<SelectEntityActionPhase>.OnPhaseEnd(SelectEntityActionPhase phase)
        {
            cinemachineCamera.gameObject.SetActive(false);
        }
    }
}