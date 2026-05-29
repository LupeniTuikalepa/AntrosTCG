using System;
using System.Threading.Tasks;
using ATCG.Battle.Cards;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
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


        [SerializeField]
        private SpriteRenderer[] interactableRenderers;

        public EntityAddress Address => Aspect.EntityAddress;
        public bool IsSelected => Manager.IsSelected(this);

        public T Aspect { get; private set; }
        public RuntimeEntityManager Manager { get; private set; }
        public bool IsHovered { get; private set; }
        public Condition IsInteractable { get; private set; }



        protected virtual void Awake()
        {
            IsInteractable = new Condition();
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
            OnEntitySelected?.Invoke();
        }

        void IRuntimeEntity.OnDeselected()
        {
            OnDeselected();
            OnEntityDeselected?.Invoke();
        }

        private void UpdateRenderers()
        {
            Color targetColor = Color.aliceBlue;

            if (!IsInteractable)
            {
                
            }
        }
    }
}