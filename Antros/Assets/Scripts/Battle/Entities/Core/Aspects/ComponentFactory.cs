using System;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities
{
    public sealed class ComponentFactory<T> : IComponentFactory where T : struct, IEntityComponent
    {
        public int ComponentID => ComponentID<T>.ID;

        private readonly Func<T> createComponent;

        public ComponentFactory(Func<T> createComponent)
        {
            this.createComponent = createComponent;
        }

        public void CreateComponent(EntityAddress address)
        {
            T component = createComponent();
            address.world.AddComponent(address.entity, in component);
        }
    }
}