using System;
using System.Threading.Tasks;
using ATCG.Battle.Cards;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Runtime;
using ATCG.Metrics;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract partial class RuntimeEntity<T> : MonoBehaviour, IRuntimeEntity where T : IEntityAspect
    {
        public event Action<T> OnEntityConnected;
        public event Action<T> OnEntityDisconnected;

        public event Action OnEntitySelected;
        public event Action OnEntityDeselected;

        public RuntimeBattlePlayer RuntimeBattlePlayer => Manager.RuntimeBattlePlayer;
        public IBattlePlayer BattlePlayer => Manager.BattlePlayer;
        public LocalBattlePlayer LocalBattlePlayer => Manager.LocalBattlePlayer;
        public BattlePhase BattlePhase => LocalBattlePlayer.BattlePhase;

        public EntityAddress Address => Aspect.EntityAddress;
        public bool IsSelected => Manager.IsSelected(this);

        public T Aspect { get; private set; }


        public RuntimeEntityManager Manager { get; private set; }

        public bool IsHovered { get; private set; }

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
        }

        protected virtual void OnDisable()
        {
            PhaseManager.Unregister<ISelectEntityPhase>(this);
        }

        public virtual async Awaitable Spawn(RuntimeEntityManager manager, T aspect)
        {
            Aspect = aspect;
            Manager = manager;

            await Awaitable.MainThreadAsync();
            OnEntityConnected?.Invoke(aspect);
        }


        public virtual void Disconnect()
        {
            T last = Aspect;

            Aspect = default;
            Manager = null;

            OnEntityDisconnected?.Invoke(last);
        }


        public void Select() => Manager.Select(this);

        public void UnSelect() => Manager.Unselect(this);


        void IRuntimeEntity.OnSelected()
        {
            OnSelected();
            Model.EnableRenderingLayer(GameMetrics.Current.SelectedRenderingLayer);
            OnEntitySelected?.Invoke();
        }

        void IRuntimeEntity.OnDeselected()
        {
            OnDeselected();
            Model.DisableRenderingLayer(GameMetrics.Current.SelectedRenderingLayer);
            OnEntityDeselected?.Invoke();
        }

    }
}