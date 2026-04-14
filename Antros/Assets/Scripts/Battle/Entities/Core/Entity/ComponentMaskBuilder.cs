using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities
{
    public struct ComponentMaskBuilder
    {
        private ComponentMask mask;

        public ComponentMaskBuilder With<T>() where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (id != -1)
                mask.Set(id);

            return this;
        }

        public static implicit operator ComponentMask(ComponentMaskBuilder builder)
        {
            return builder.mask;
        }
    }
}