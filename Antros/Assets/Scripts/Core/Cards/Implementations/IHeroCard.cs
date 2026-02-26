namespace ATCG.Cards.Implementations
{
    public interface IHeroCard : IGameCard
    {
        int MaxHealth { get; }
        int DeathCost { get; }
        int Speed { get; }
        int Strength { get; }
    }
}