using System;
using System.Collections.Generic;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using Helteix.Tools;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public class EntityActionUIController : MonoBehaviour,
        IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
        [SerializeField]
        private EntityActionUIPanel start;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private RuntimeEntityManager runtimeEntityManager;

        public IRuntimeEntity RuntimeEntity => runtimeEntity;

        public RuntimeBattlePlayer RuntimeBattlePlayer { get; private set; }

        private IRuntimeEntity runtimeEntity;
        private readonly Stack<EntityActionUIPanel> openedPanels = new();

        private void Start()
        {
            Hide();
        }

        private void OnEnable()
        {
            runtimeEntityManager.OnEntitySelected += OnHeroSelected;
            runtimeEntityManager.OnEntityDeselected += OnHeroDeselected;
        }

        private void OnDisable()
        {
            runtimeEntityManager.OnEntitySelected -= OnHeroSelected;
            runtimeEntityManager.OnEntityDeselected -= OnHeroDeselected;
        }

        private void OnHeroSelected(IRuntimeEntity entity)
        {
            runtimeEntity = entity;

            Show();
            start.Build();
            if (start.IsEmpty())
                return;

            Open(start);
        }

        private void OnHeroDeselected(IRuntimeEntity entity)
        {
            CloseAllAsync().FireAndForget();
            Hide();

            runtimeEntity = null;
        }


        public void Show()
        {
            Tween.StopAll(canvasGroup);
            Tween.Alpha(canvasGroup, 1, .15f, Ease.OutExpo);
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            Tween.StopAll(canvasGroup);
            Tween.Alpha(canvasGroup, 0, .15f, Ease.OutExpo);
            canvasGroup.blocksRaycasts = false;
        }

        public void Open(EntityActionUIPanel panel) => OpenAsync(panel).FireAndForget();
        public async Awaitable OpenAsync(EntityActionUIPanel panel)
        {
            if (openedPanels.TryPeek(out var openedPanel))
                await openedPanel.OnClose();

            openedPanels.Push(panel);
            await panel.OnOpen();
        }

        public async Awaitable CloseAllAsync()
        {
            if (!openedPanels.TryPop(out var panel))
                return;

            await panel.OnClose();

            openedPanels.Clear();
            if (runtimeEntityManager.IsSelected(runtimeEntity))
                runtimeEntityManager.Unselect(runtimeEntity);
        }

        public void CloseLast() => CloseLastAsync().FireAndForget();

        public async Awaitable CloseLastAsync()
        {
            if (!openedPanels.TryPop(out var panel))
                return;

            await panel.OnClose();
            if (openedPanels.TryPeek(out var openedPanel))
                await openedPanel.OnOpen();
            else if (runtimeEntityManager.IsSelected(runtimeEntity))
                runtimeEntityManager.Unselect(runtimeEntity);
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            RuntimeBattlePlayer = runtimeBattlePlayer;
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
        }
    }
}