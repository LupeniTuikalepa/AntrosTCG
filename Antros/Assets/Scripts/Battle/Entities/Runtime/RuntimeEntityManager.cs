using System;
using System.Collections.Generic;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players;
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
        public Priority<IEntitySelectionController> SelectionController { get; private set; }

        public RuntimeBattlePlayer RuntimeBattlePlayer { get; private set; }
        public IBattlePlayer BattlePlayer { get; private set; }

        /// <summary>
        /// Casts the player to a local player if it's one. The entity manager could be without one.
        /// </summary>
        public LocalBattlePlayer LocalBattlePlayer => BattlePlayer as LocalBattlePlayer;

        private List<Entity> selectedEntities;


        private Dictionary<Entity, IRuntimeEntity> runtimeEntities;



        private void Awake()
        {
            runtimeEntities = new Dictionary<Entity, IRuntimeEntity>();
            selectedEntities = new ();

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

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            this.RuntimeBattlePlayer = runtimeBattlePlayer;
            this.BattlePlayer = player;
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            RuntimeBattlePlayer = null;
            BattlePlayer = null;
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

        public void Select(Entity entity)
        {
            if(runtimeEntities.TryGetValue(entity, out IRuntimeEntity runtimeEntity))
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
                RegisterRuntimeEntity(runtimeEntity);

                EnsureSelectableSlot(1);
                selectedEntities.Add(runtimeEntity.Address);
                runtimeEntity.OnSelected();
                OnEntitySelected?.Invoke(runtimeEntity);
            }
        }

        public void Unselect(Entity entity)
        {
            if(runtimeEntities.TryGetValue(entity, out IRuntimeEntity runtimeEntity))
                Unselect(runtimeEntity);
        }
        public void Unselect(IRuntimeEntity runtimeEntity)
        {
            RegisterRuntimeEntity(runtimeEntity);
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
            if(SelectionController.Value == null)
                return;

            int maxSelectableEntities = SelectionController.Value.MaxSelectableEntities;

            if (quantity >= maxSelectableEntities)
                quantity = maxSelectableEntities;
            if(quantity <= 0)
                return;

            int remaining = maxSelectableEntities - selectedEntities.Count;
            for (int i = remaining; i < quantity; i++)
            {
                Unselect(selectedEntities[0]);
            }
        }

        public bool IsSelected(IRuntimeEntity runtimeEntity) => selectedEntities.Contains(runtimeEntity.Address);



        #endregion


    }
}