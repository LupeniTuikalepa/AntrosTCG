using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Runtime
{
    public partial class RuntimeEntityManager : MonoBehaviour,
        IRuntimeBattlePlayerComponent<LocalBattlePlayer>,
        IPhaseListener<SelectCellPhase>
    {
        public event Action<IRuntimeEntity> OnEntitySelected;
        public event Action<IRuntimeEntity> OnEntityDeselected;

        public IEnumerable<IRuntimeEntity> SelectedEntities
        {
            get
            {
                foreach (Entity entity in selectedEntities)
                {
                    if(runtimeEntities.TryGetValue(entity, out IRuntimeEntity runtimeEntity))
                        yield return runtimeEntity;
                }
            }
        }

        [field: SerializeField]
        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }

        [ShowInInspector, ReadOnly]
        public Condition Selectable { get; private set; }

        [ShowInInspector, ReadOnly]
        public Priority<int> MaxSelectables { get; private set; }

        public RuntimeBattlePlayer RuntimeBattlePlayer { get; private set; }

        private List<Entity> selectedEntities;

        private Dictionary<Entity, IRuntimeEntity> runtimeEntities;

        public World ConnectedWorld { get; private set; }

        private void Awake()
        {
            this.Register();

            runtimeEntities = new Dictionary<Entity, IRuntimeEntity>();
            selectedEntities = new ();

            Selectable = new Condition();
            MaxSelectables = new Priority<int>();

            Selectable.AddOnValueChangeCallback(ctx =>
            {
                if (!ctx)
                    ClearSelection();
            });
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            this.RuntimeBattlePlayer = runtimeBattlePlayer;
            GameCommandManager.Instance.Register(this);
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            GameCommandManager.Instance.Unregister(this);
        }

        public void RegisterRuntimeEntity(IRuntimeEntity runtimeEntity)
        {
            runtimeEntities[runtimeEntity.Address] = runtimeEntity;
        }

        public bool UnregisterRuntimeEntity(IRuntimeEntity runtimeEntity)
        {
            return runtimeEntities.Remove(runtimeEntity.Address);
        }


        public void Select(Entity entity)
        {
            if(runtimeEntities.TryGetValue(entity, out IRuntimeEntity runtimeEntity))
                Select(runtimeEntity);
        }

        public void Select(IRuntimeEntity runtimeEntity)
        {
            if (!Selectable)
                return;

            RegisterRuntimeEntity(runtimeEntity);

            EnsureSelectableSlot(1);
            selectedEntities.Add(runtimeEntity.Address);
            runtimeEntity.OnSelected();

            OnEntitySelected?.Invoke(runtimeEntity);
        }

        public void Unselect(Entity entity)
        {
            if(runtimeEntities.TryGetValue(entity, out IRuntimeEntity runtimeEntity))
                Unselect(runtimeEntity);
        }
        public void Unselect(IRuntimeEntity runtimeEntity)
        {
            RegisterRuntimeEntity(runtimeEntity);

            if (!selectedEntities.Remove(runtimeEntity.Address))
                return;

            runtimeEntity.OnDeselected();
            OnEntityDeselected?.Invoke(runtimeEntity);
        }

        public void ClearSelection()
        {
            using (ListPool<Entity>.Get(out var copy))
            {
                copy.AddRange(selectedEntities);
                foreach (Entity entity in copy)
                    Unselect(entity);
            }
        }

        private void EnsureSelectableSlot(int quantity)
        {
            if (quantity >= MaxSelectables)
                quantity = MaxSelectables;
            if(quantity <= 0)
                return;

            int remaining = MaxSelectables - selectedEntities.Count;
            for (int i = remaining; i < quantity; i++)
            {
                Unselect(selectedEntities[0]);
            }
        }

        public bool IsSelected(IRuntimeEntity runtimeEntity) => selectedEntities.Contains(runtimeEntity.Address);


        private void OnInteractableLayerChanged(LayerMask mask)
        {
            foreach ((_, IRuntimeEntity value) in runtimeEntities)
            {
                bool isInMask = (mask & (1 << value.gameObject.layer)) != 0;
                value.SetInteractableState(isInMask);
            }
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseBegin(SelectCellPhase phase)
        {
            Selectable.AddCondition(phase.MainChannelKey, false);

            if (RuntimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
            {
                runtimeLocalBattlePlayer.Camera.InteractableLayer.OnValueChanged += OnInteractableLayerChanged;
            }
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseEnd(SelectCellPhase phase)
        {
            Selectable.RemoveCondition(phase.MainChannelKey);
        }
    }
}