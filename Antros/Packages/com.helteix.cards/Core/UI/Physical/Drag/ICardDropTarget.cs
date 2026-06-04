namespace Helteix.Cards.UI.Physical.Drag
{
    public interface ICardDropTarget<in TCard> : ICardDropTargetPointer<TCard> where TCard : ICard
    {
        ICardDropTarget<TCard> ICardDropTargetPointer<TCard>.DropTarget => this;

        public int Priority { get; }
        bool Accepts(ICardUI<TCard> cardUI);


        public void OnCardEnter(ICardUI<TCard> cardUI);
        public void OnCardExit(ICardUI<TCard> cardUI);

        public void OnCardDrop(ICardUI<TCard> cardUI);
        void OnCardHover(ICardUI<TCard> cardUI);
    }
}