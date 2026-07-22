using System;
using ATCG.Battle.Entities.Components;
using Unity.Burst;

namespace ATCG.Battle.Entities
{
    [Serializable]
    public readonly struct Entity : IEquatable<Entity>
    {
        public bool IsValid => id >= 0;

        public static readonly Entity None = new Entity(-1);

        public readonly int id;

        // Bumped by World every time this id's slot is destroyed and recycled. Equality
        // (Equals/GetHashCode/==) deliberately stays id-only — existing Dictionary<Entity,>
        // / HashSet<Entity> usage across the codebase depends on that. Generation is
        // instead checked exclusively by World.IsAlive, so a handle captured before a
        // destroy+recycle cycle reads as dead instead of silently aliasing whatever new
        // entity now sits at the same id.
        public readonly int generation;

        public Entity(int id)
        {
            this.id = id;
            this.generation = 0;
        }

        public Entity(int id, int generation)
        {
            this.id = id;
            this.generation = generation;
        }


        public EntityAddress ToAddress(World world) => new EntityAddress(world, this);

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
            if (!IsAlive(world))
                return false;

            return world.TryGetROComponent(this, out component);
        }

        public bool TryGetComponent<T>(World world, out ComponentRef<T> componentRef) where T : struct, IEntityComponent
        {
            componentRef = default;
            if (!IsAlive(world))
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

        public override string ToString() => id.ToString();

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