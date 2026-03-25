using System;
using ATCG.Battle.Entities.Core.Components;

namespace ATCG.Battle.Entities.Core
{
    public class World
    {
        private int nextId;
        private readonly int maxEntities;
        private SparseSet<EntityMeta> entities;

        private IComponentStore[] stores;


        public World(int maxEntities, int maxComponentStores = 64)
        {
            this.maxEntities = maxEntities;

            stores = new IComponentStore[maxComponentStores];
            entities = new SparseSet<EntityMeta>(maxEntities);
            nextId = 0;
        }

        public Entity CreateEntity()
        {
            var id = nextId++;
            entities.Set(id, new EntityMeta
            {
                isActive = true,
            });
            return new Entity(id);
        }

        public bool IsAlive(Entity e) => entities.Has(e.id);

        public bool IsActive(Entity e) => IsAlive(e) && entities[e].isActive;

        public void Activate(Entity e)
        {
            ref EntityMeta meta = ref entities[e];
            meta.isActive = true;
        }

        public void Deactivate(Entity e)
        {
            ref EntityMeta meta = ref entities[e];
            meta.isActive = false;
        }

        public ref readonly EntityMeta GetMeta(Entity e) => ref entities[e];

        public bool TryGetComponent<T>(Entity e, out ComponentRef<T> componentRef) where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is ComponentStore<T> store)
            {
                int idx = store.GetRefIndex(e);
                if (idx != -1)
                {
                    componentRef = new ComponentRef<T>(store, id);
                    return true;
                }
            }

            componentRef = default;
            return false;
        }
        public ref T GetComponent<T>(Entity e) where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is ComponentStore<T> store && store.Has(e.id))
                return ref store.GetRef(e.id);

            return ref ComponentStore<T>.DefaultComponent;
        }

        public bool AddComponent<T>(Entity e) where T : struct, IEntityComponent
            => AddComponent<T>(e, new());

        public bool AddComponent<T>(Entity e, in T component) where T : struct, IEntityComponent
        {
            ref EntityMeta meta = ref entities[e];
            if (meta.HasComponent<T>())
                return false;

            int id = ComponentID<T>.ID;
            if(id >= stores.Length)
                Array.Resize(ref stores, stores.Length * 2);

            stores[id] ??= new ComponentStore<T>(maxEntities);
            if (stores[id] is ComponentStore<T> store)
            {
                if (store.Has(e.id))
                    return false;

                store.Set(e.id, component);
                meta.AddComponentToMask<T>();
                return true;
            }

            return false;
        }
        public bool RemoveComponent<T>(Entity e) where T : struct, IEntityComponent
        {
            ref EntityMeta meta = ref entities[e];
            if (!meta.HasComponent<T>())
                return false;

            int id = ComponentID<T>.ID;
            if (stores[id] is ComponentStore<T> store && store.Has(e.id))
            {
                store.Remove(e.id);
                meta.RemoveComponentFromMask<T>();
                return true;
            }

            return false;
        }

        public void DestroyEntity(Entity e)
        {
            // Retire l'entité de tous les stores
            foreach (var store in stores)
                store?.Remove(e.id);

            // Retire l'entité du registre
            entities.Remove(e.id);
        }

    }
}