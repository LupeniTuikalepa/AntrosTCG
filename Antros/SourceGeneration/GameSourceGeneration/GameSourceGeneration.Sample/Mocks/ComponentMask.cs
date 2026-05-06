using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities;

public struct ComponentMask
{
    public static ComponentMask With<T>() where T : IEntityComponent
    {
        return default;
    }


}