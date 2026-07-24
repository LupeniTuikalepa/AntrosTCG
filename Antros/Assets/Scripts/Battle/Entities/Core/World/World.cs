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

        // Per-id generation counter, bumped every time a slot is destroyed. Lets IsAlive
        // tell "this id is alive" apart from "this id is alive, but it's a DIFFERENT
        // entity than the one this handle was captured from" — ids get recycled
        // immediately (see RemoveEntity/CreateEntity), so any Entity/EntityAddress held
        // across a destroy+recreate cycle would otherwise silently start referring to
        // the new occupant with no way to detect it.
        private int[] generations;

        private IComponentStore[] stores;

        public World(int maxEntities, int maxComponentStores = 64)
        {
            this.maxEntities = maxEntities;

            stores = new IComponentStore[maxComponentStores];
            entities = new SparseSet<EntityMeta>(maxEntities);
            generations = new int[maxEntities];
            nextId = 0;
        }

        /// <summary>Current generation for an id — 0 for an id never yet recycled.</summary>
        public int GetGeneration(int id) => id >= 0 && id < generations.Length ? generations[id] : 0;

        private void EnsureGenerationCapacity(int id)
        {
            if (id < generations.Length)
                return;

            int newSize = generations.Length;
            while (newSize <= id) newSize *= 2;
            Array.Resize(ref generations, newSize);
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
            EnsureGenerationCapacity(id);
            entities.Set(id, new EntityMeta { isActive = true });
            return new Entity(id, generations[id]);
        }

        public Entity CreateEntity(EntityAddress parent)
        {
            Entity entity = CreateEntity();
            AddOrSetComponent(entity, new ChildOfComponent(parent));

            return entity;
        }
        public bool IsAlive(in Entity e)
        {
            return entities.Has(e.id) && GetGeneration(e.id) == e.generation;
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
            int id = EnsureStore<ChildOfComponent>();
            
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

            // Bump BEFORE the id goes back on the free list: the next CreateEntity() to
            // pop this id reads generations[id] to stamp the new Entity, so any handle
            // captured before this Remove now carries a stale generation and fails
            // IsAlive instead of silently matching whatever gets created here next.
            EnsureGenerationCapacity(e);
            generations[e]++;
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
                        CollectDeathDependencies(new Entity(entity, GetGeneration(entity)), childOfComponents, entitiesToDestroy);
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