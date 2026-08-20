using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Status.Status;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities.Runtime.Components;
using ATCG.Battle.Entities.Runtime.VFX;
using ATCG.Battle.GameModes;
using ATCG.Battle.PassiveSystem.Runtimes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Runtime;
using ATCG.Capacities.Data.Status;
using ATCG.Metrics;
using ATCG.Passives.Datas;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools;
using Helteix.Tools.Phases;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime
{
    [SelectionBase]
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
        public Transform PassiveRoot { get; private set; }
        
        [field: SerializeField, BoxGroup("UI")]
        public Transform HoveredRoot { get; private set; }

        private Dictionary<StatusData, RuntimeStatus> statusDatas;
        private Dictionary<PassiveData, RuntimePassive> passives;
        private List<IRuntimeEntityComponent<T>> components;

        protected virtual void Awake()
        {
	        components = new List<IRuntimeEntityComponent<T>>(GetComponentsInChildren<IRuntimeEntityComponent<T>>());
	        Models = new LinkedRendererGroup(GetComponentsInChildren<LinkedRenderer>());
            IsInteractable = new Condition();
            statusDatas = new();
            passives = new();
        }

        protected virtual void CollectComponents()
        {
	        Models = new LinkedRendererGroup(GetComponentsInChildren<LinkedRenderer>());
        }

        protected virtual void OnEnable()
        {
            PhaseManager.Register(this);
            CommandManager.Register(this);
        }

        protected virtual void OnDisable()
        {
            PhaseManager.Unregister(this);
            CommandManager.Unregister(this);
        }

        public virtual async Awaitable Spawn(RuntimeEntityManager manager, T aspect)
        {
            Aspect = aspect;
            Manager = manager;

            Manager.RegisterRuntimeEntity(this);

            await Awaitable.MainThreadAsync();
            OnEntityConnected?.Invoke(aspect);
            
            foreach (var entityComponent in components)
            {
	            entityComponent.Connect(aspect,this);
            }
        }

        public virtual async Awaitable Despawn()
        {
	        foreach (var entityComponent in components)
	        {
		        entityComponent.Disconnect(Aspect,this);
	        }
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