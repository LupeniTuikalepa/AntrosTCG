using System;
using System.ComponentModel;

namespace ATCG.Battle.Entities.Core.Components
{
    public class ComponentStore<T> : IComponentStore where T : struct, IEntityComponent
    {
        public const int NONE = -1;

        public static T DefaultComponent = default;

        private T[] dense; // les composants, contigus en mémoire
        private int[] denseToEntity; // dense[i] appartient à quelle entité ?
        private int[] sparse; // entityId → index dans dense (-1 = absent)
        private int count;

        public int Count => count;

        public ComponentStore(int maxEntities)
        {
            dense = new T[maxEntities];
            denseToEntity = new int[maxEntities];
            sparse = new int[maxEntities];
            Array.Fill(sparse, NONE);
        }

        public bool Has(int entityId) => sparse[entityId] != NONE;

        public ref T GetRef(int entityId)
        {
            int index = sparse[entityId];
            if (index == NONE)
                throw new Exception($"Entity {entityId} has no component {typeof(T).Name}");
            return ref dense[index];
        }

        public  ref T GetRefWithIndex(int index) => ref dense[index];

        public int GetRefIndex(int entityId) => sparse[entityId];

        public void Set(int entityId, in T component)
        {
            var index = sparse[entityId];
            if (index == NONE)
            {
                index = count++;
                sparse[entityId] = index;
                denseToEntity[index] = entityId;
            }

            dense[index] = component;
        }

        //Suppression (swap avec le dernier pour garder dense compact)
        public void Remove(int entityId)
        {
            var index = sparse[entityId];
            if (index == NONE)
                return;

            var lastIndex = count - 1;
            var lastEntityId = denseToEntity[lastIndex];

            //Écrase le trou avec le dernier élément
            dense[index] = dense[lastIndex];
            denseToEntity[index] = lastEntityId;
            sparse[lastEntityId] = index;

            //Nettoie la dernière case
            sparse[entityId] = NONE;
            count--;
        }

        //Itération cache-friendly
        public Span<T> AllComponents => dense.AsSpan(0, count);
        public Span<int> AllEntities => denseToEntity.AsSpan(0, count);
    }
}