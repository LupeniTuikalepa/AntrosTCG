using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities;

public static class ComponentMaskExtensions
{
    public static ComponentMask With<T>(this ComponentMask mask) where T : IEntityComponent
    {
        return ComponentMask.With<T>();
    }
}