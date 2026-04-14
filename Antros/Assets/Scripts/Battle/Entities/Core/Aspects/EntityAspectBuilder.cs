using System;
using System.Collections;
using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities
{
    public readonly struct EntityAspectBuilder<T> : IEnumerable, IDisposable where T : struct, IEntityAspect
    {
        private readonly Dictionary<int, IComponentFactory> factories;

        public EntityAspectBuilder(Dictionary<int, IComponentFactory>buffer = null)
        {
            this.factories = buffer ?? DictionaryPool<int, IComponentFactory>.Get();
        }

        public IEnumerator GetEnumerator() => factories.GetEnumerator();

        void IDisposable.Dispose()
        {
            Dispose();
        }

        private void Dispose()
        {
            DictionaryPool<int, IComponentFactory>.Release(factories);
        }

        public void Add<TComponent>(ComponentFactory<TComponent> factory) where TComponent : struct, IEntityComponent
            => factories[factory.ComponentID] = factory;

        public T CreateAndDispose(World world)
        {
            T  aspect = Create(world);
            Dispose();

            return aspect;
        }
        
        public T Create(World world)
        {
            EntityAddress address = new EntityAddress(world, world.CreateEntity());
            T aspect = new T
            {
                EntityAddress = address
            };

            ComponentMask mask = aspect.ComponentMask;
            world.EnsureStores(mask);

            foreach (int id in mask)
            {
                if (factories.TryGetValue(id, out IComponentFactory factory))
                    factory.CreateComponent(address);
                else
                {
                    IComponentStore store = world.GetStore(id);
                    store.Add(address.entity);
                }
            }

            return aspect;
        }
    }
}