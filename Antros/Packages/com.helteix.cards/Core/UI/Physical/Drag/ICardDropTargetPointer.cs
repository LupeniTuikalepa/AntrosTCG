namespace Helteix.Cards.UI.Physical.Drag
{
    public interface ICardDropTargetPointer<in TCard> where TCard : ICard
    {
        ICardDropTarget<TCard> DropTarget { get; }
    }
}