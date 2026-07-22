namespace ATCG.Cards.Implementations
{
    public interface IConstructionCard : IGameCard
    {
        int MaxHealth { get; }
        int DeathCost { get; }
    }
}