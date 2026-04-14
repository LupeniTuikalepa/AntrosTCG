using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Lookups
{
    public interface IComponentLookupFilter<T> where T : struct, IEntityComponent
    {
        bool IsValid(in ComponentRef<T> componentRef);
    }
}