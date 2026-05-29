using ATCG.Battle.Entities;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface IEntityFilter
    {
        bool Accepts(EntityAddress address);
    }
}