using System;
using System.Collections.Generic;
using System.Reflection;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Premade;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities
{
    public partial class World
    {
        private static readonly MethodInfo GetOrCreateStoreMethodInfos = typeof(World)
            .GetMethod(nameof(EnsureStore), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly int maxEntities;
        private SparseSet<EntityMeta> entities;

        // Recycle destroyed entity IDs to avoid exhausting the ID space.
        private readonly Stack<int> freeIds = new();
        private int nextId;

        private IComponentStore[] stores;

        public World(int maxEntities, int maxComponentStores = 64)
        {
            this.maxEntities = maxEntities;

            stores = new IComponentStore[maxComponentStores];
            entities = new SparseSet<EntityMeta>(maxEntities);
            nextId = 0;
        }

        public ReadOnlySpan<EntityMeta> Metas => entities.AllElements;
        public ReadOnlySpan<int> Entities => entities.AllIDs;

        public void EnsureStores(ComponentMask mask)
        {
            foreach (int id in mask)
            {
                if (stores[id] != null)
                    continue;

                Type componentType = ComponentRegistry.GetTypeForComponentID(id);
                if (componentType is not { IsValueType: true })
                    continue;

                // Reflection is required here since the store type is generic and only known at runtime.
                // MethodInfo instances are cached per type to avoid repeated MakeGenericMethod calls.
                MethodInfo methodInfo = GetOrCreateStoreMethodInfos.MakeGenericMethod(componentType);
                methodInfo.Invoke(this, null);
            }
        }

        public Entity CreateEntity()
        {
            int id = freeIds.Count > 0 ? freeIds.Pop() : nextId++;
            entities.Set(id, new EntityMeta { isActive = true });
            return new Entity(id);
        }

        public Entity CreateEntity(EntityAddress parent)
        {
            Entity entity = CreateEntity();
            AddOrSetComponent(entity, new ChildOfComponent(parent));

            return entity;
        }
        public bool IsAlive(in Entity e)
        {
            return entities.Has(e.id);
        }

        public bool IsActive(in Entity e)
        {
            return IsAlive(e) && entities[e].isActive;
        }

        public void Activate(in Entity e)
        {
            EntityMeta meta = entities[e];
            meta.isActive = true;
            entities[e] = meta;
        }

        public void Deactivate(in Entity e)
        {
            EntityMeta meta = entities[e];
            meta.isActive = false;
            entities[e] = meta;
        }

        public EntityMeta GetMeta(in Entity e)
        {
            return entities[e];
        }

        public void DestroyEntity(in Entity e)
        {
            int id = ComponentID<ChildOfComponent>.ID;

            if (stores[id] is not ComponentStore<ChildOfComponent> store)
                return;

            using (HashSetPool<int>.Get(out var toDestroy))
            {
                toDestroy.Add(e);
                CollectDeathDependencies(e, store, toDestroy);

                foreach (int entity in toDestroy)
                    RemoveEntity(entity);
            }
        }

        private void RemoveEntity(int e)
        {
            for (int i = 0; i < stores.Length; i++)
            {
                IComponentStore store = stores[i];
                store?.Remove(e);
            }

            entities.Remove(e);
            freeIds.Push(e);
        }

        private void CollectDeathDependencies(Entity e, ComponentStore<ChildOfComponent> childOfComponents, HashSet<int> entitiesToDestroy)
        {
            for (int i = 0; i < childOfComponents.Count; i++)
            {
                ChildOfComponent component = childOfComponents.AllComponents[i];
                int entity = childOfComponents.AllEntities[i];

                if (component.entity == e)
                {
                    if (entitiesToDestroy.Add(entity))
                    {
                        CollectDeathDependencies(new Entity(entity), childOfComponents, entitiesToDestroy);
                    }
                    else
                    {
                        Debug.LogError("[Entity] A circular relation was detected when destroying entity " + e + ".");
                        return;
                    }
                }
            }
        }
    }
}