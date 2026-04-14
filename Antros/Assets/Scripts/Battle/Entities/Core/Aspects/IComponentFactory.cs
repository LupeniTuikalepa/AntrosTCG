namespace ATCG.Battle.Entities
{
    public interface IComponentFactory
    {
        void CreateComponent(EntityAddress address);
    }
}