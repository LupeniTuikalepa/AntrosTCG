using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Animations;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Cards.Implementations;
using ATCG.Metrics;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Cinemachine;
using UnityEditor.Animations;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public partial class RuntimeHero : RuntimeEntity<HeroEntityAspect>, ICutsceneActor, ILocalPlayerPhaseListener<SelectEntityActionPhase>, IRuntimeEntityWithAnimator
    {
        [SerializeField, BoxGroup("UI")]
        private TMP_Text heroName;

        [field: SerializeField, BoxGroup("GameFeel"), ReadOnly]
        public Animator Animator { get; private set; }
        
        [SerializeField, BoxGroup("GameFeel"), Range(0, 30)]
        private float movementDuration;

        [SerializeField, BoxGroup("GameFeel")] private CinemachineCamera cinemachineCamera;
        
        private SelectEntityActionPhase selectPhase;

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

        private void LateUpdate()
        {
            if (selectPhase == null)
                return;
            actionUIRoot.forward = actionUIRoot.position - cinemachineCamera.transform.position;
        }


        public override async Awaitable Spawn(RuntimeEntityManager manager, HeroEntityAspect aspect)
        {
            await base.Spawn(manager, aspect);
            var herodata = aspect.HeroCard.Data;
            heroName.text = herodata.name;
            
            CollectComponents();
            manager.RegisterRuntimeEntity(this);

            if (RuntimeBattleGrid.TryGetBattleCellAt(aspect.GridMemberComponent.coordinates, out RuntimeBattleCell cell))
            {
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                await Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }

            int playerID = BattlePhase.GetPlayerNumber(aspect.Player);
            RenderingLayerMask mask = RenderingLayerMask.GetMask($"Player{playerID + 1}");
            if (mask.value != 0)
            {
                foreach (Renderer model in Models)
                {
                    model.EnableRenderingLayer(mask);
                }
            }

            if (LocalBattlePlayer.TryGetRuntime(out RuntimeLocalBattlePlayer runtimeLocalBattlePlayer))
                cinemachineCamera.OutputChannel = runtimeLocalBattlePlayer.Camera.Component.GetOutputChannel();
        }

        protected override void CollectComponents()
        {
	        base.CollectComponents();
	        Animator = GetComponentInChildren<Animator>();
        }

        public void Despawn(RuntimeEntityManager manager)
        {
            manager.UnregisterRuntimeEntity(this);
        }


        void IPhaseListener<SelectEntityActionPhase>.OnPhaseBegin(SelectEntityActionPhase phase)
        {
            selectPhase = phase;

            if (IsSelected)
            {
                cinemachineCamera.gameObject.SetActive(true);
            }
        }

        void IPhaseListener<SelectEntityActionPhase>.OnPhaseEnd(SelectEntityActionPhase phase)
        {
            cinemachineCamera.gameObject.SetActive(false);
            selectPhase = null;
        }
    }
}