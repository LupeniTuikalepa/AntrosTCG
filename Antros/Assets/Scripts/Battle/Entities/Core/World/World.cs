using System;
using System.Reflection;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities
{
    public partial class World
    {
        private static readonly MethodInfo GetOrCreateStoreMethodInfos = typeof(World)
            .GetMethod(nameof(EnsureStore), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly int maxEntities;
        private SparseSet<EntityMeta> entities;
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

                MethodInfo methodInfo = GetOrCreateStoreMethodInfos.MakeGenericMethod(componentType);
                methodInfo.Invoke(this, null);
            }
        }

        public T CreateEntity<T>(EntityAspectBuilder<T> builder) where T : struct, IEntityAspect
        {
            return builder.Create(this);
        }
        
        public Entity CreateEntity()
        {
            int id = nextId++;
            entities.Set(id, new EntityMeta
            {
                isActive = true
            });
            return new Entity(id);
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
            // Retire l'entité de tous les stores
            foreach (IComponentStore store in stores)
                store?.Remove(e.id);

            // Retire l'entité du registre
            entities.Remove(e.id);
        }
    }
}