using UnityEngine;

namespace Helteix.Cards.UI.Physical.Components
{
    public abstract class CardUIComponent<TCard> : CardUIComponent,  ICardUIComponent<TCard> where TCard : ICard
    {
        public TCard Current { get; private set; }

        public ICardUI<TCard> UI { get; private set; }

        protected override void Awake()
        {
            UI = GetComponent<ICardUI<TCard>>();
            base.Awake();
        }
        public virtual void Connect(TCard current)
        {
            if(Current != null)
                Disconnect(Current);

            Current = current;
        }

        public virtual void Disconnect(TCard current)
        {
            if(!current.Equals(Current))
                return;

            Current = default;
        }
    }

    public class CardUIComponent : MonoBehaviour, ICardUIComponent
    {
        public CardHolderUI HolderUI => CardUI.HolderUI;

        public ICardUI CardUI { get; set; }

        [SerializeField]
        private bool registerOnEnable = true;
        [SerializeField]
        private bool unregisteredOnDisable = true;

        protected virtual void Awake()
        {
            CardUI = GetComponent<ICardUI>();
        }

        private void OnEnable()
        {
            if(CardUI != null && registerOnEnable)
                CardUI.RegisterComponent(this);
        }

        private void OnDisable()
        {
            if(CardUI != null && unregisteredOnDisable)
                CardUI.UnregisterComponent(this);
        }
    }
}