using System;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Lookups;
using UnityEngine;

namespace ATCG.Battle.Entities
{
    public partial class World
    {
        public IComponentStore GetStore(int index)
        {
            return stores[index];
        }

        public bool TryGetStore<T>(out ComponentStore<T> store) where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is ComponentStore<T> s)
            {
                store = s;
                return true;
            }

            store = null;
            return false;
        }

        public ComponentsLookupResult<T, TFilter> LookupComponents<TFilter, T>(in TFilter filter)
            where TFilter : IComponentLookupFilter<T>
            where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is not ComponentStore<T> store)
                return default;

            return new ComponentsLookupResult<T, TFilter>(store, filter, this);
        }

        public ComponentsLookupResult<T> LookupComponents<T>() where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is not ComponentStore<T> store)
                return default;

            return new ComponentsLookupResult<T>(store, this);
        }

        public bool HasComponent<T>(Entity e) where T : struct, IEntityComponent
        {
            EntityMeta meta = entities[e];
            return meta.HasComponent<T>();
        }

        public bool TryGetROComponent<T>(Entity e, out T component) where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is ComponentStore<T> store)
            {
                int idx = store.EntityIDToIndex(e);
                if (idx != -1)
                {
                    component = store.GetRefWithIndex(idx);
                    return true;
                }
            }

            component = default;
            return false;
        }

        public bool TryGetComponent<T>(Entity e, out ComponentRef<T> componentRef) where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is ComponentStore<T> store)
            {
                int idx = store.EntityIDToIndex(e);
                if (idx != -1)
                {
                    componentRef = new ComponentRef<T>(this, store, id);
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

        public bool AddOrSetComponent<T>(Entity e) where T : struct, IEntityComponent
        {
            return AddOrSetComponent(e, new T());
        }

        public bool AddOrSetComponent<T>(Entity e, in T component) where T : struct, IEntityComponent
        {
            ref EntityMeta meta = ref entities[e];
            if (meta.HasComponent<T>())
                return false;

            int id = EnsureStore<T>();
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

        private int EnsureStore<T>() where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (id >= stores.Length)
            {
                Debug.Log($"Resizing store for Component {typeof(T).Name} with id {id}");
                Array.Resize(ref stores, stores.Length * 2);
            }

            if (stores[id] != null)
                return id;

            Debug.Log($"Creating new store for Component {typeof(T).Name} with id {id}");
            stores[id] = new ComponentStore<T>(maxEntities);
            return id;
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
    }
}