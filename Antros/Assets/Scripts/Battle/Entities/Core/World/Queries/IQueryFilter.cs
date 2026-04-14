namespace ATCG.Battle.Entities.Queries
{
    public interface IQueryFilter
    {
        bool Evaluate(Entity entity, World world);
    }
}