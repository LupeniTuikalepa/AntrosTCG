namespace ATCG.Cards.Implementations
{
    public interface IHeroCard : IGameCard
    {
        int Health { get; }
        int DeathCost { get; }
        int Speed { get; }
        int Strength { get; }
    }
}