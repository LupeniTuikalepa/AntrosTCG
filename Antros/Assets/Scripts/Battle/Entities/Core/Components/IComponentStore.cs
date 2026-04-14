namespace ATCG.Battle.Entities.Components
{
    public interface IComponentStore
    {
        void Add(int entityId);

        void Remove(int entityId);
    }
}