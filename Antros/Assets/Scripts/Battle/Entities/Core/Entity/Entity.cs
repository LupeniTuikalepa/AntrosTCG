using System;
using ATCG.Battle.Entities.Components;
using Unity.Burst;

namespace ATCG.Battle.Entities
{
    [Serializable]
    public readonly struct Entity : IEquatable<Entity>
    {
        public static readonly Entity None = new Entity(-1);

        public readonly int id;

        public Entity(int id)
        {
            this.id = id;
        }


        public bool IsAlive(World world)
        {
            return world.IsAlive(this);
        }

        public bool HasComponent<T>(World world) where T : struct, IEntityComponent
        {
            return world.HasComponent<T>(this);
        }

        public ref T GetComponent<T>(World world) where T : struct, IEntityComponent
        {
            return ref world.GetComponent<T>(this);
        }

        public bool TryGetROComponent<T>(World world, out T component) where T : struct, IEntityComponent
        {
            component = default;
            if (IsAlive(world))
                return false;

            return world.TryGetROComponent(this, out component);
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
            return world.AddOrSetComponent(this, component);
        }

        public bool RemoveComponent<T>(World world) where T : struct, IEntityComponent
        {
            return world.RemoveComponent<T>(this);
        }

        public static implicit operator int(Entity entity)
        {
            return entity.id;
        }

        public bool Equals(Entity other)
        {
            return id == other.id;
        }

        [BurstDiscard]
        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}