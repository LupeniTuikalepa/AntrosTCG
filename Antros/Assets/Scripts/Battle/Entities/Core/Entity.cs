using ATCG.Battle.Entities.Core.Components;
using UnityEngine.UIElements;

namespace ATCG.Battle.Entities.Core
{
    public readonly struct Entity
    {
        public readonly int id;

        public Entity(int id)
        {
            this.id = id;
        }

        public bool IsAlive(World world) => world.IsAlive(this);

        public ref T GetComponent<T>(World world) where T : struct, IEntityComponent
        {
            return ref world.GetComponent<T>(this);
        }

        public bool TryGetComponent<T>(World world, out ComponentRef<T> componentRef) where T : struct, IEntityComponent
        {
            componentRef = default;
            if (IsAlive(world))
                return false;

            return world.TryGetComponent(this, out componentRef);
        }

        public bool AddComponent<T>(World world, in T component) where T : struct, IEntityComponent
        {
            return world.AddComponent(this, component);
        }

        public bool RemoveComponent<T>(World world) where T : struct, IEntityComponent
        {
            return world.RemoveComponent<T>(this);
        }

        public static implicit operator int (Entity entity) => entity.id;
    }
}