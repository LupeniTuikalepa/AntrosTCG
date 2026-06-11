using System;
using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class PlayerInteractions : RuntimeLocalPlayerComponent
    {
        public event Action<PlayerInteractable> OnSelection;
        public event Action<PlayerInteractable> OnDeselection;

        [ShowInInspector, ReadOnly]
        public Priority<IInteractionController> CurrentController { get; private set; }

        [ShowInInspector, ReadOnly]
        public Condition CanInteract { get; private set; }

        public IReadOnlyList<PlayerInteractable> ActiveSelection => activeSelection;

        public Priority<PointerController> PointerController { get; private set; }

        private List<PlayerInteractable> activeSelection;
        private List<PlayerInteractable> interactables;

        public PlayerInteractable HoveredInteractable { get; private set; }

        protected void Awake()
        {
            interactables = new List<PlayerInteractable>();
            activeSelection = new List<PlayerInteractable>();

            CanInteract = new Condition();
        }

        protected override void Connect(LocalBattlePlayer player)
        {

        }

        protected override void Disconnect(LocalBattlePlayer player)
        {

        }


        public bool Register(PlayerInteractable interactable)
        {
            if(interactables.Contains(interactable))
                return false;

            interactables.Add(interactable);
            return true;
        }

        public bool Unregister(PlayerInteractable interactable)
        {
            return interactables.Remove(interactable);
        }

        public bool Select(PlayerInteractable interactable)
        {
            if (!CanInteract)
                return false;

            if (!IsRegistered(interactable))
            {
                Debug.LogError("[Player Interactions] Attempting to select an interactable that is not registered!", this);
                return false;
            }

            if(!CurrentController.Value.Validate(interactable))
                return false;

            EnsureSelectableSlot(1);

            activeSelection.Add(interactable);
            interactable.OnSelected();

            OnSelection?.Invoke(interactable);
            return true;
        }

        public bool Unselect(PlayerInteractable interactable)
        {
            if (!IsRegistered(interactable))
            {
                Debug.LogError("[Player Interactions] Attempting to unselect an interactable that is not registered!", this);
                return false;
            }

            Register(interactable);

            if (!activeSelection.Remove(interactable))
                return false;

            interactable.OnDeselected();
            OnDeselection?.Invoke(interactable);
            return true;
        }


        public void ClearSelection()
        {
            using (ListPool<PlayerInteractable>.Get(out var copy))
            {
                copy.AddRange(activeSelection);
                foreach (PlayerInteractable interactable in copy)
                    Unselect(interactable);
            }
        }

        private void EnsureSelectableSlot(int quantity)
        {
            int maxSelectables = CurrentController.Value.MaxSelectables;
            if (quantity >= maxSelectables)
                quantity = maxSelectables;
            if(quantity <= 0)
                return;

            int remaining = maxSelectables - activeSelection.Count;
            for (int i = remaining; i < quantity; i++)
                Unselect(activeSelection[0]);
        }

        public bool IsRegistered(PlayerInteractable interactable) => interactables.Contains(interactable);

        public bool IsSelected(PlayerInteractable interactable) => activeSelection.Contains(interactable);

        public bool IsActive(PlayerInteractable interactable) =>
            IsRegistered(interactable) &&
            CanInteract &&
            CurrentController.Value.Validate(interactable);
    }
}