using System;
using ATCG.Battle.Players.Local.Runtime;
using Helteix.ChanneledProperties.Conditions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ATCG.Battle
{
    public class PlayerInteractable : MonoBehaviour
    {
        [ShowInInspector]
        public Condition IsInteractable { get; private set; }

        public RuntimeLocalBattlePlayer RLBPlayer { get; private set; }


        public PlayerInteractions Interactions => RLBPlayer.Interactions;

        [SerializeField, LabelText("On Selected")]
        private UnityEvent onSelectedUV;
        [SerializeField, LabelText("On Unselected")]
        private UnityEvent onUnselectedUV;

        private void Awake()
        {
            IsInteractable = new Condition();
            RLBPlayer = GetComponentInParent<RuntimeLocalBattlePlayer>();
        }

        private void OnEnable()
        {
            if (RLBPlayer != null)
            {
                PlayerInteractions interactions = RLBPlayer.Interactions;
                interactions.Register(this);
            }
        }

        private void OnDisable()
        {
            if (RLBPlayer != null)
            {
                PlayerInteractions interactions = RLBPlayer.Interactions;
                interactions.Unregister(this);
            }
        }

        public void Select()
        {
            if (RLBPlayer != null)
            {
                PlayerInteractions interactions = RLBPlayer.Interactions;
                interactions.Select(this);
            }
        }

        public void Unselect()
        {
            if (RLBPlayer != null)
            {
                PlayerInteractions interactions = RLBPlayer.Interactions;
                interactions.Unselect(this);
            }
        }

        public bool IsActive() => Interactions.IsActive(this);

        public virtual void OnSelected() { Debug.Log("On Selected"); }
        public virtual void OnDeselected() { }

        public virtual void OnActivates() { }
        public virtual void OnDeactivates() { }
    }
}