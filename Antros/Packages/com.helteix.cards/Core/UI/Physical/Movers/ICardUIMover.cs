namespace Helteix.Cards.UI.Physical.Movers
{
    public interface ICardUIMover
    {
        int Priority { get; }
        void MoveCard(CardHolderUI holderUI, ICardUI cardUI);
    }
}