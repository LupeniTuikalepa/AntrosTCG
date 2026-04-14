using System;
using ATCG.Battle.Entities.Components;
using Unity.Burst;

namespace ATCG.Battle.Entities
{
    public readonly struct EntityAddress : IEquatable<EntityAddress>
    {
        public EntityMeta Meta => world.GetMeta(entity);

        public readonly World world;
        public readonly Entity entity;

        public bool IsValid => world != null && entity.id != -1;

        public EntityAddress(World world, Entity entity)
        {
            this.world = world;
            this.entity = entity;
        }

        public bool HasAllComponents(in ComponentMask componentMask)
        {
            return world.GetMeta(in entity).HasAllComponents(in componentMask);
        }

        public bool HasAnyComponents(in ComponentMask componentMask)
        {
            return world.GetMeta(in entity).HasAnyComponents(in componentMask);
        }

        public bool HasComponents<T>() where T : struct, IEntityComponent
        {
            return world.GetMeta(in entity).HasComponent<T>();
        }

        public bool HasComponent<T>() where T : struct, IEntityComponent
        {
            return world.HasComponent<T>(entity);
        }

        public ref T GetComponent<T>() where T : struct, IEntityComponent
        {
            return ref world.GetComponent<T>(entity);
        }

        public bool TryGetComponent<T>(out ComponentRef<T> componentRef) where T : struct, IEntityComponent
        {
            return world.TryGetComponent(entity, out componentRef);
        }

        public bool TryConvertToAspect<T>(out T aspect) where T : IEntityAspect, new()
        {
            return entity.TryConvertToAspect(world, out aspect);
        }

        public T ToAspect<T>() where T : IEntityAspect, new()
        {
            return entity.ToAspect<T>(world);
        }

        public bool ToAspect<T>(out T aspect) where T : IEntityAspect, new()
        {
            aspect = ToAspect<T>();
            return aspect.IsValid;
        }

        public void Destroy()
        {
            world.DestroyEntity(entity);
        }

        [BurstDiscard]
        public bool Equals(EntityAddress other)
        {
            return this == other;
        }

        [BurstDiscard]
        public override bool Equals(object obj)
        {
            return obj is EntityAddress other && Equals(other);
        }

        [BurstDiscard]
        public override int GetHashCode()
        {
            return HashCode.Combine(entity.id, world);
        }

        [BurstDiscard]
        public static bool operator ==(EntityAddress a, EntityAddress b)
        {
            return a.world == b.world && a.entity == b.entity;
        }

        [BurstDiscard]
        public static bool operator !=(EntityAddress a, EntityAddress b)
        {
            return !(a == b);
        }

        public static implicit operator Entity(EntityAddress address) => address.entity;
        public static implicit operator World(EntityAddress address) => address.world;

    }
}