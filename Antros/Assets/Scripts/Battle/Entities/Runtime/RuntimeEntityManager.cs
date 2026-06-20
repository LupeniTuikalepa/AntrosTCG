using System;
using System.Collections.Generic;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Runtime;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Runtime
{
    public partial class RuntimeEntityManager : MonoBehaviour, IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
        public event Action<IRuntimeEntity> OnEntityHoverBegin;
        public event Action<IRuntimeEntity> OnEntityHoverEnd;
        public event Action<IRuntimeEntity> OnEntitySelected;
        public event Action<IRuntimeEntity> OnEntityDeselected;

        public IEnumerable<IRuntimeEntity> SelectedEntities
        {
            get
            {
                foreach (Entity entity in selectedEntities)
                {
                    if (runtimeEntities.TryGetValue(entity, out IRuntimeEntity runtimeEntity))
                        yield return runtimeEntity;
                }
            }
        }

        public IRuntimeEntity HoveredEntity
        {
            get
            {
                if (hoveredEntity.IsValid &&
                    runtimeEntities.TryGetValue(hoveredEntity, out IRuntimeEntity runtimeEntity))
                    return runtimeEntity;

                return null;
            }
        }

        [field: SerializeField]
        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }

        [ShowInInspector, ReadOnly]
        public Condition Selectable { get; private set; }

        [ShowInInspector, ReadOnly]
        public Priority<IEntitySelectionController> SelectionController { get; private set; }

        public IRuntimeBattlePlayer<LocalBattlePlayer> RuntimeBattlePlayer { get; private set; }
        public LocalBattlePlayer LocalBattlePlayer => RuntimeBattlePlayer.BattlePlayer;


        private List<Entity> selectedEntities;
        private Entity hoveredEntity;


        private Dictionary<Entity, IRuntimeEntity> runtimeEntities;


        private void Awake()
        {
            runtimeEntities = new Dictionary<Entity, IRuntimeEntity>();
            selectedEntities = new();

            Selectable = new Condition();
            SelectionController = new Priority<IEntitySelectionController>(new DefaultSelectionController());

            Selectable.AddOnValueChangeCallback(ctx =>
            {
                if (!ctx)
                    ClearSelection();
            });
        }

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(
            IRuntimeBattlePlayer<LocalBattlePlayer> runtimeBattlePlayer)
        {
            RuntimeBattlePlayer = runtimeBattlePlayer;
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(
            IRuntimeBattlePlayer<LocalBattlePlayer> runtimeBattlePlayer)
        {
            RuntimeBattlePlayer = null;
        }

        public void RegisterRuntimeEntity(IRuntimeEntity runtimeEntity)
        {
            runtimeEntities[runtimeEntity.Address] = runtimeEntity;
        }

        public bool UnregisterRuntimeEntity(IRuntimeEntity runtimeEntity)
        {
            return runtimeEntities.Remove(runtimeEntity.Address);
        }

        public bool TryGetRuntimeEntity(EntityAddress address, out IRuntimeEntity runtimeEntity) =>
            TryGetRuntimeEntity(address.entity, out runtimeEntity);

        public bool TryGetRuntimeEntity(Entity entity, out IRuntimeEntity runtimeEntity)
        {
            if (entity.IsValid)
            {
                return runtimeEntities.TryGetValue(entity, out runtimeEntity);
            }

            runtimeEntity = null;
            return false;
        }


        #region Selection

        public void BeginHover(IRuntimeEntity runtimeEntity)
        {
            if (!Selectable)
                return;

            if (hoveredEntity.IsValid && TryGetRuntimeEntity(hoveredEntity, out IRuntimeEntity entity))
                EndHover(entity);

            EntityAddress address = runtimeEntity.Address;
            SelectionController.Value.OnHoverBegin(runtimeEntity, ref address);

            if (TryGetRuntimeEntity(address, out runtimeEntity))
            {
                runtimeEntity.OnHovered();
                OnEntityHoverBegin?.Invoke(runtimeEntity);
            }
        }

        public void EndHover(IRuntimeEntity runtimeEntity)
        {
            if (!Selectable)
                return;
            if (hoveredEntity.IsValid && TryGetRuntimeEntity(hoveredEntity, out IRuntimeEntity entity))
            {
                EntityAddress address = runtimeEntity.Address;
                SelectionController.Value.OnHoverEnd(runtimeEntity, ref address);

                if (TryGetRuntimeEntity(address, out runtimeEntity))
                {
                    runtimeEntity.OnUnhovered();
                    OnEntityHoverEnd?.Invoke(runtimeEntity);
                }
            }
        }

        public void Select(Entity entity)
        {
            if (runtimeEntities.TryGetValue(entity, out IRuntimeEntity runtimeEntity))
                Select(runtimeEntity);
        }

        public void Select(IRuntimeEntity runtimeEntity)
        {
            if (!Selectable)
                return;

            EntityAddress address = runtimeEntity.Address;
            SelectionController.Value.OnSelected(runtimeEntity, ref address);

            if (TryGetRuntimeEntity(address, out runtimeEntity))
            {
                EnsureSelectableSlot(1);
                selectedEntities.Add(runtimeEntity.Address);
                runtimeEntity.OnSelected();
                OnEntitySelected?.Invoke(runtimeEntity);
            }
        }

        public void Unselect(Entity entity)
        {
            if (runtimeEntities.TryGetValue(entity, out IRuntimeEntity runtimeEntity))
                Unselect(runtimeEntity);
        }

        public void Unselect(IRuntimeEntity runtimeEntity)
        {
            EntityAddress address = runtimeEntity.Address;
            SelectionController.Value.OnUnselected(runtimeEntity, ref address);

            if (TryGetRuntimeEntity(address, out runtimeEntity))
            {
                if (!selectedEntities.Remove(runtimeEntity.Address))
                    return;

                runtimeEntity.OnDeselected();

                OnEntityDeselected?.Invoke(runtimeEntity);
            }
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
            if (SelectionController.Value == null)
                return;

            int maxSelectableEntities = SelectionController.Value.MaxSelectableEntities;

            if (quantity >= maxSelectableEntities)
                quantity = maxSelectableEntities;
            if (quantity <= 0)
                return;

            int remaining = maxSelectableEntities - selectedEntities.Count;
            for (int i = remaining; i < quantity; i++)
            {
                Unselect(selectedEntities[0]);
            }
        }

        public bool IsHovered(IRuntimeEntity runtimeEntity) => hoveredEntity == runtimeEntity.Address.entity;

        public bool IsSelected(IRuntimeEntity runtimeEntity) => selectedEntities.Contains(runtimeEntity.Address);

        #endregion
    }
}