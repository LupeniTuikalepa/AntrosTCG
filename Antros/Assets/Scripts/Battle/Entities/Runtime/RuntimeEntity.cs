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
        public Renderer[] Models { get; private set; }

        [field: SerializeField, BoxGroup("UI")]
        public Transform actionUIRoot { get; private set; }


        protected virtual void Awake()
        {
            Models = GetComponentsInChildren<Renderer>();
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
            OnSelected();
            for (int i = 0; i < Models.Length; i++)
            {
                if(Models[i] != null)
                    Models[i].EnableRenderingLayer(GameMetrics.Current.SelectedRenderingLayer);
            }
            OnEntitySelected?.Invoke();
        }

        void IRuntimeEntity.OnDeselected()
        {
            OnDeselected();
            for (int i = 0; i < Models.Length; i++)
            {
                if(Models[i] != null)
                    Models[i].DisableRenderingLayer(GameMetrics.Current.SelectedRenderingLayer);
            }
            OnEntityDeselected?.Invoke();
        }
        void IRuntimeEntity.OnHovered()
        {
            OnHovered();
            for (int i = 0; i < Models.Length; i++)
            {
                if (Models[i] != null)
                    Models[i].EnableRenderingLayer(GameMetrics.Current.HoverRenderingLayer);
            }

        }

        void IRuntimeEntity.OnUnhovered()
        {
            OnUnhovered();
            for (int i = 0; i < Models.Length; i++)
            {
                if (Models[i] != null)
                    Models[i].DisableRenderingLayer(GameMetrics.Current.HoverRenderingLayer);
            }
        }
    }
}