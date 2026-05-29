namespace Helteix.Cards.Collections
{
    /// <summary>
    /// Non-generic marker interface for card containers.
    /// Do not implement this interface directly — implement <see cref="ICardContainer{T}"/> instead.
    /// This interface exists solely to allow non-generic references to containers (e.g. collections, constraints).
    /// </summary>
    public interface ICardContainer
    {
    }

    public interface ICardContainer<in T> : ICardContainer
    {
        bool TryAddCard(T card, bool notify = true);
        bool TryRemoveCard(T card, bool notify = true);
    }
}