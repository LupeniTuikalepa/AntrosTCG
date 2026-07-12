using System;
using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities.Runtime.Status;
using ATCG.Battle.Entities.Runtime.VFX;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Runtime;
using ATCG.Capacities.Data.Status;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
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


        [field: ShowInInspector, ReadOnly, BoxGroup("Debug")]
        public LinkedRendererGroup Models { get; private set; }

        [field: SerializeField, BoxGroup("UI")]
        public Transform actionUIRoot { get; private set; }

        [field: SerializeField, BoxGroup("UI")]
        public Transform statusRoot { get; private set; }
        [field: SerializeField, BoxGroup("UI")]
        public Transform HoveredRoot { get; private set; }

        private Dictionary<StatusData, RuntimeStatus> statusDatas;

        protected virtual void Awake()
        {
            Models = new LinkedRendererGroup(GetComponentsInChildren<LinkedRenderer>());
            IsInteractable = new Condition();
            statusDatas = new();
        }

        protected virtual void OnEnable()
        {
            PhaseManager.Register(this);
            CommandManager.RegisterListener(this);
        }

        protected virtual void OnDisable()
        {
            PhaseManager.Unregister(this);
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
            OnSelected();
            foreach (var model in Models.GetAll())
            {
                if(model.Renderer != null)
                    model.Renderer.EnableRenderingLayer(GameMetrics.Current.SelectedRenderingLayer);
            }
            OnEntitySelected?.Invoke();
        }

        void IRuntimeEntity.OnDeselected()
        {
            OnDeselected();


            foreach (var model in Models.GetAll())
            {
                if(model.Renderer != null)
                    model.Renderer.DisableRenderingLayer(GameMetrics.Current.SelectedRenderingLayer);
            }
            OnEntityDeselected?.Invoke();
        }
        void IRuntimeEntity.OnHovered()
        {
            OnHovered();
            foreach (var model in Models.GetAll())
            {
                if(model.Renderer != null)
                    model.Renderer.EnableRenderingLayer(GameMetrics.Current.HoverRenderingLayer);
            }

        }

        void IRuntimeEntity.OnUnhovered()
        {
            OnUnhovered();
            foreach (var model in Models.GetAll())
            {
                if(model.Renderer != null)
                    model.Renderer.DisableRenderingLayer(GameMetrics.Current.HoverRenderingLayer);
            }
        }
    }
}