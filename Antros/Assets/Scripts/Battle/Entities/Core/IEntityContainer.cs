namespace ATCG.Battle.Entities.Core
{
    public interface IEntityContainer
    {
        Entity Entity { get; }
    }

    public static class EntityContainerExtensions
    {
        public static Entity ToEntity(this IEntityContainer container) => container.Entity;
    }
}