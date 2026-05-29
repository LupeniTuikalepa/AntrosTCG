namespace Helteix.Cards.UI.Physical.Drag
{
    public interface ICardDropTarget<in TCard> where TCard : ICard
    {
        public int Priority { get; }
        bool Accepts(ICardUI<TCard> cardUI);

        public void OnCardEnter(ICardUI<TCard> cardUI);
        public void OnCardExit(ICardUI<TCard> cardUI);

        public void OnCardDrop(ICardUI<TCard> cardUI);
        void OnCardHover(ICardUI<TCard> cardUI);
    }
}