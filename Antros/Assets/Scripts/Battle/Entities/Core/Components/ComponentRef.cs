namespace ATCG.Battle.Entities.Components
{
    public readonly struct ComponentRef<T> where T : struct, IEntityComponent
    {
        public readonly World world;
        public readonly ComponentStore<T> store;
        public readonly int entityID;

        public Entity Entity => new(entityID);

        public EntityAddress Address => new EntityAddress(world, Entity);

        public ComponentRef(World world, ComponentStore<T> store, int entityID)
        {
            this.world = world;
            this.store = store;
            this.entityID = entityID;
        }

        public ref T GetValue()
        {
            return ref store.GetRef(entityID);
        }

        public static implicit operator T(ComponentRef<T> @ref)
        {
            return @ref.GetValue();
        }
    }
}