using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Lookups
{
    public interface IFilter<T> where T : struct, IEntityComponent
    {
        bool IsValid(in ComponentRef<T> componentRef);
    }
}