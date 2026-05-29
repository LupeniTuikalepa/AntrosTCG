using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helteix.Tools
{
    [Serializable]
    public struct DynamicBuffer<T>
    {
        [SerializeField]
        private T[] innerArray;

        public T this[int i] => innerArray[i];

        public int Length { get; private set; }

        public DynamicBuffer(int baseCapacity)
        {
            innerArray = new T[baseCapacity];
            Length = 0;
        }

        public void CopyFrom(List<T> list)
        {
            for (int i = 0; i < Length; i++)
                innerArray[i] = default;

            int listCount = list.Count;
            if (innerArray.Length < listCount)
                innerArray = new T[Mathf.NextPowerOfTwo(listCount)];

            Length = listCount;
            list.CopyTo(innerArray);
        }

        public void Clear()
        {
            T d = default(T);

            for (int i = 0; i < innerArray.Length; i++)
                innerArray[i] = d;
        }

        public static implicit operator T[](DynamicBuffer<T> buffer) => buffer.innerArray;
    }
}