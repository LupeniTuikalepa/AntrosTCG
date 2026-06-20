using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Runtime;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract partial class RuntimeEntity<T> : MonoBehaviour, IRuntimeEntity where T : IEntityAspect
    {
        public event Action<T> OnEntityConnected;
        public event Action<T> OnEntityDisconnected;

        public event Action OnEntitySelected;
        public event Action OnEntityDeselected;

        public IRuntimeBattlePlayer<LocalBattlePlayer> RuntimeBattlePlayer => Manager.RuntimeBattlePlayer;
        public IBattlePlayer BattlePlayer => Manager.LocalBattlePlayer;
        public LocalBattlePlayer LocalBattlePlayer => Manager.LocalBattlePlayer;
        public BattlePhase BattlePhase => LocalBattlePlayer.BattlePhase;

        public EntityAddress Address => Aspect.EntityAddress;

        public bool IsSelected => Manager.IsSelected(this);

        public bool IsHovered => Manager.IsHovered(this);
        public T Aspect { get; private set; }


        public RuntimeEntityManager Manager { get; private set; }


        public Condition IsInteractable { get; private set; }


        [field: SerializeField]
        public MeshRenderer Model { get; private set; }

        [field: SerializeField]
        public Transform actionUIRoot { get; private set; }


        protected virtual void Awake()
        {
            IsInteractable = new Condition();
        }

        protected virtual void OnEnable()
        {
            PhaseManager.Register<ISelectEntityPhase>(this);
            CommandManager.RegisterListener(this);
        }

        protected virtual void OnDisable()
        {
            PhaseManager.Unregister<ISelectEntityPhase>(this);
            CommandManager.UnregisterListener(this);
        }

        public virtual async Awaitable Spawn(RuntimeEntityManager manager, T aspect)
        {
            Aspect = aspect;
            Manager = manager;

            Manager.RegisterRuntimeEntity(this);

            await Awaitable.MainThreadAsync();
            OnEntityConnected?.Invoke(aspect);
        }

        public virtual async Awaitable Despawn()
        {
            await Awaitable.MainThreadAsync();

            T last = Aspect;
            Manager.UnregisterRuntimeEntity(this);
            Aspect = default;
            Manager = null;

            OnEntityDisconnected?.Invoke(last);
        }


        public void Select() => Manager.Select(this);

        public void UnSelect() => Manager.Unselect(this);

        void IRuntimeEntity.OnSelected()
        {
            Model.EnableRenderingLayer(GameMetrics.Current.SelectedRenderingLayer);
            OnSelected();
            OnEntitySelected?.Invoke();
        }

        void IRuntimeEntity.OnDeselected()
        {
            Model.DisableRenderingLayer(GameMetrics.Current.SelectedRenderingLayer);
            OnDeselected();
            OnEntityDeselected?.Invoke();
        }
        void IRuntimeEntity.OnHovered()
        {
            OnHovered();
            Model.EnableRenderingLayer(GameMetrics.Current.HoverRenderingLayer);
        }

        void IRuntimeEntity.OnUnhovered()
        {
            OnUnhovered();
            Model.DisableRenderingLayer(GameMetrics.Current.HoverRenderingLayer);
        }
    }
}