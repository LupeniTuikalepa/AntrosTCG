using Helteix.Cards.Collections;

namespace Helteix.Cards
{

    public interface ICard
    {
        ICardContainer Container { get; internal set; }
    }

    public abstract class Card : ICard
    {
        public ICardContainer Container { get; internal set; }

        ICardContainer ICard.Container
        {
            get => Container;
            set => Container = value;
        }
    }
}