namespace ATCG.Battle.Cards.Capacities.Behaviours
{
    public interface ICapacityBehaviour<in TData>
    {
        bool Accepts(TData data);
    }
}