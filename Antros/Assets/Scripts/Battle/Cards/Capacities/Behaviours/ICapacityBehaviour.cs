namespace ATCG.Battle.Cards.Capacities.Patterns
{
    public interface ICapacityBehaviour<in TData>
    {
        bool Accepts(TData data);
    }

}