using System;

namespace ATCG.Battle.Entities.Components
{
    public class SparseSet<T>
    {
        private T[] dense;
        private int[] denseToElement;
        private int[] sparse;

        public SparseSet(int initialCapacity = 64)
        {
            dense = new T[initialCapacity];
            denseToElement = new int[initialCapacity];
            sparse = new int[initialCapacity];
            Count = 0;

            Array.Fill(sparse, -1);
        }

        public int Count { get; private set; }

        public ReadOnlySpan<T> AllElements => dense.AsSpan(0, Count);
        public ReadOnlySpan<int> AllIDs => denseToElement.AsSpan(0, Count);


        public ref T this[int id] => ref GetRef(id);

        public bool Has(int id)
        {
            if (id >= sparse.Length) return false;
            return sparse[id] != -1;
        }

        public ref T GetRef(int id)
        {
            if (id >= sparse.Length || sparse[id] == -1)
                throw new Exception($"Element with ID : {id} does not exist.");
            return ref dense[sparse[id]];
        }

        public bool TryGetRef(int id, out int index)
        {
            if (id >= sparse.Length)
            {
                index = -1;
                return false;
            }

            index = sparse[id];
            return index != -1;
        }

        public void Set(int id, in T component)
        {
            // Resize sparse si l'entityID dépasse
            if (id >= sparse.Length)
                GrowSparse(id + 1);

            int index = sparse[id];
            if (index == -1)
            {
                // Resize dense si plus de place
                if (Count >= dense.Length)
                    GrowDense();

                index = Count++;
                sparse[id] = index;
                denseToElement[index] = id;
            }

            dense[index] = component;
        }

        public void Remove(int id)
        {
            if (id >= sparse.Length) return;

            int index = sparse[id];
            if (index == -1) return;

            int lastIndex = Count - 1;
            int lastEntityId = denseToElement[lastIndex];

            dense[index] = dense[lastIndex];
            denseToElement[index] = lastEntityId;
            sparse[lastEntityId] = index;

            sparse[id] = -1;
            Count--;
        }


        private void GrowSparse(int minCapacity)
        {
            int newSize = sparse.Length;
            while (newSize < minCapacity) newSize *= 2;

            int[] newSparse = new int[newSize];
            Array.Copy(sparse, newSparse, sparse.Length);
            Array.Fill(newSparse, -1, sparse.Length, newSize - sparse.Length);
            sparse = newSparse;
        }

        private void GrowDense()
        {
            int newSize = dense.Length * 2;

            var newDense = new T[newSize];
            int[] newDenseToElement = new int[newSize];

            Array.Copy(dense, newDense, Count);
            Array.Copy(denseToElement, newDenseToElement, Count);

            dense = newDense;
            denseToElement = newDenseToElement;
        }
    }
}