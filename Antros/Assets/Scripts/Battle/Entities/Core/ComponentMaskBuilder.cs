using ATCG.Battle.Entities.Core.Components;

namespace ATCG.Battle.Entities.Core
{
    public struct ComponentMaskBuilder
    {
        private ComponentMask mask;

        public ComponentMaskBuilder With<T>() where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if(id != -1)
                mask.Set(id);

            return this;
        }

        public static implicit operator ComponentMask(ComponentMaskBuilder builder) => builder.mask;
    }
}