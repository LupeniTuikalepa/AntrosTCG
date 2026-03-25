namespace ATCG.Battle.Entities.Core.Components
{
    public readonly struct ComponentRef<T> where T : struct, IEntityComponent
    {
        private readonly ComponentStore<T> store;
        private readonly int id;

        public ComponentRef(ComponentStore<T> store, int id)
        {
            this.store = store;
            this.id = id;
        }

        public ref T Value => ref store.GetRef(id);

        public static implicit operator T(ComponentRef<T> @ref) => @ref.Value;
    }
}