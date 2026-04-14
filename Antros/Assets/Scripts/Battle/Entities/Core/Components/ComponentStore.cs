using System;

namespace ATCG.Battle.Entities.Components
{
    public class ComponentStore<T> : IComponentStore where T : struct, IEntityComponent
    {
        public const int NONE = -1;

        public static T DefaultComponent = default;
        private readonly int[] denseToEntity; // dense[i] appartient à quelle entité ?
        private readonly int[] sparse; // entityId → index dans dense (-1 = absent)

        private T[] dense; // les composants, contigus en mémoire

        public ComponentStore(int maxEntities)
        {
            dense = new T[maxEntities];
            denseToEntity = new int[maxEntities];
            sparse = new int[maxEntities];
            Array.Fill(sparse, NONE);
        }

        public int Count { get; private set; }

        //Itération cache-friendly
        public ReadOnlySpan<T> AllComponents => dense.AsSpan(0, Count);
        public ReadOnlySpan<int> AllEntities => denseToEntity.AsSpan(0, Count);

        public void Add(int entityId)
        {
            T component = new();
            Set(entityId, in component);
        }

        //Suppression (swap avec le dernier pour garder dense compact)
        public void Remove(int entityId)
        {
            int index = sparse[entityId];
            if (index == NONE)
                return;

            int lastIndex = Count - 1;
            int lastEntityId = denseToEntity[lastIndex];

            //Écrase le trou avec le dernier élément
            dense[index] = dense[lastIndex];
            denseToEntity[index] = lastEntityId;
            sparse[lastEntityId] = index;

            //Nettoie la dernière case
            sparse[entityId] = NONE;
            Count--;
        }

        public bool Has(int entityId)
        {
            return sparse[entityId] != NONE;
        }

        public ref T GetRef(int entityId)
        {
            int index = sparse[entityId];
            if (index == NONE)
                throw new Exception($"Entity {entityId} has no component {typeof(T).Name}");
            return ref dense[index];
        }

        public ref T GetRefWithIndex(int index)
        {
            return ref dense[index];
        }

        public int EntityIDToIndex(int entityId)
        {
            return sparse[entityId];
        }

        public int IndexToEntityID(int index)
        {
            return denseToEntity[index];
        }

        public void Set(int entityId, in T component)
        {
            int index = sparse[entityId];
            if (index == NONE)
            {
                index = Count++;
                sparse[entityId] = index;
                denseToEntity[index] = entityId;
            }

            dense[index] = component;
        }
    }
}