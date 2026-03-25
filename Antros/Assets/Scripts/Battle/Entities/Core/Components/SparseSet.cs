using System;

namespace ATCG.Battle.Entities.Core.Components
{
    public class SparseSet<T>
    {
        private T[] dense;
        private int[] denseToElement;
        private int[] sparse;
        private int count;

        public int Count => count;

        public Span<T> AllElements => dense.AsSpan(0, count);
        public Span<int> AllIDs => denseToElement.AsSpan(0, count);

        public SparseSet(int initialCapacity = 64)
        {
            dense = new T[initialCapacity];
            denseToElement = new int[initialCapacity];
            sparse = new int[initialCapacity];
            count = 0;

            Array.Fill(sparse, -1);
        }


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
            // Resize sparse si l'id dépasse
            if (id >= sparse.Length)
                GrowSparse(id + 1);

            var index = sparse[id];
            if (index == -1)
            {
                // Resize dense si plus de place
                if (count >= dense.Length)
                    GrowDense();

                index = count++;
                sparse[id] = index;
                denseToElement[index] = id;
            }

            dense[index] = component;
        }

        public void Remove(int id)
        {
            if (id >= sparse.Length) return;

            var index = sparse[id];
            if (index == -1) return;

            var lastIndex = count - 1;
            var lastEntityId = denseToElement[lastIndex];

            dense[index] = dense[lastIndex];
            denseToElement[index] = lastEntityId;
            sparse[lastEntityId] = index;

            sparse[id] = -1;
            count--;
        }


        private void GrowSparse(int minCapacity)
        {
            int newSize = sparse.Length;
            while (newSize < minCapacity) newSize *= 2;

            var newSparse = new int[newSize];
            Array.Copy(sparse, newSparse, sparse.Length);
            Array.Fill(newSparse, -1, sparse.Length, newSize - sparse.Length);
            sparse = newSparse;
        }

        private void GrowDense()
        {
            int newSize = dense.Length * 2;

            var newDense = new T[newSize];
            var newDenseToElement = new int[newSize];

            Array.Copy(dense, newDense, count);
            Array.Copy(denseToElement, newDenseToElement, count);

            dense = newDense;
            denseToElement = newDenseToElement;
        }
    }
}