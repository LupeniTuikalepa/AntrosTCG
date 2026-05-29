namespace Helteix.Cards.UI.Physical.Components
{
    public interface ICardUIComponent
    {
        public ICardUI CardUI { get; set; }
    }

    public interface ICardUIComponent<in TCard> : ICardUIComponent where TCard : ICard
    {
        void Connect(TCard current);
        void Disconnect(TCard current);
    }
}