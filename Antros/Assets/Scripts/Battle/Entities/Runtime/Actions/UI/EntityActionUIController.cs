using System;
using System.Collections.Generic;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using ATCG.Utilities;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public class EntityActionUIController : MonoBehaviour,
        ILocalPlayerPhaseListener<SelectEntityActionPhase>,
        IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
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

        public LocalBattlePlayer LocalBattlePlayer { get; private set; }
        public RuntimeBattlePlayer RuntimeBattlePlayer { get; private set; }

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

        public void Open(EntityActionUIPanel panel) => OpenAsync(panel).FireAndForget();

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

        public void CloseLast() => CloseLastAsync().FireAndForget();

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
                CloseAllAsync().FireAndForget();
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
        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            LocalBattlePlayer = player;
            RuntimeBattlePlayer = runtimeBattlePlayer;

        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            LocalBattlePlayer = null;
        }

    }
}