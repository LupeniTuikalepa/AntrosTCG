namespace ATCG.Battle.Entities.Queries
{
    public interface IEntityQueryFilter
    {
        bool Evaluate(Entity entity, World world);
    }
}