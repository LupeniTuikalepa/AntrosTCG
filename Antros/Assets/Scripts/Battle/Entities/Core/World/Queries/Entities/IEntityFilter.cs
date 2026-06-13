namespace ATCG.Battle.Entities.Queries
{
    public interface IEntityFilter
    {
        bool Accepts(EntityAddress entityAddress);
    }
}