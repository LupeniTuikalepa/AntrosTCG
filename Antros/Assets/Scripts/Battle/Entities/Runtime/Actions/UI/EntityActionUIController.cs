using System.Collections.Generic;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Utilities;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public class EntityActionUIController : RuntimeLocalPlayerComponent,
        ILocalPlayerPhaseListener<SelectEntityActionPhase>
    {
        LocalBattlePlayer ILocalPlayerPhaseListener<SelectEntityActionPhase>.LocalBattlePlayer => Player;

        [SerializeField]
        private EntityActionUIPanel start;

        [SerializeField]
        private CanvasGroup canvasGroup;
        
        [SerializeField]
        private RuntimeEntityManager runtimeEntityManager;

        public SelectEntityActionPhase Phase { get; private set; }

        public IRuntimeEntity RuntimeEntity
        {
            get
            {
                if (runtimeEntityManager.TryGetRuntimeEntity(Phase.entityAddress, out var runtimeEntity))
                    return runtimeEntity;

                return null;
            }
        }

        private readonly Stack<EntityActionUIPanel> openedPanels = new();

        private void Start()
        {
            canvasGroup.Hide(0);
        }

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        private void LateUpdate()
        {
            if (Phase is null || RuntimeEntity?.actionUIRoot is null)
                return;
            transform.rotation = RuntimeEntity.actionUIRoot.rotation;
        }

        public void Open(EntityActionUIPanel panel) => OpenAsync(panel).ListenForExceptions();

        public async Awaitable OpenAsync(EntityActionUIPanel panel)
        {
            if (openedPanels.TryPeek(out var openedPanel))
                await openedPanel.OnClose();

            openedPanels.Push(panel);
            await panel.OnOpen();
        }

        private async Awaitable CloseAllAsync()
        {
            if (!openedPanels.TryPop(out var panel))
                return;

            await panel.OnClose();

            openedPanels.Clear();

            Phase?.SetResult(null);
        }

        public void CloseLast() => CloseLastAsync().ListenForExceptions();

        public async Awaitable CloseLastAsync()
        {
            if (!openedPanels.TryPop(out var panel))
                return;

            await panel.OnClose();
            if (openedPanels.TryPeek(out var openedPanel))
                await openedPanel.OnOpen();
            else
                Phase?.SetResult(null);
        }

        public void Exit()
        {
            if(Phase == null)
                CloseAllAsync().ListenForExceptions();
            else
                Phase.Cancel();

            canvasGroup.Hide(.15f);
        }

        void IPhaseListener<SelectEntityActionPhase>.OnPhaseBegin(SelectEntityActionPhase phase)
        {
            Phase = phase;

            canvasGroup.Show(.15f);
            transform.position = RuntimeEntity.actionUIRoot.position;
            start.Build();
            if (start.IsEmpty())
                return;

            Open(start);
        }

        void IPhaseListener<SelectEntityActionPhase>.OnPhaseEnd(SelectEntityActionPhase phase)
        {
            if (Phase == phase)
            {
                Exit();
                canvasGroup.Hide(.15f);
            }
        }

        protected override void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
        }

        protected override void Disconnect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
        {
        }

    }
}