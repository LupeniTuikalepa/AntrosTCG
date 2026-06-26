using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Battle.Entities.Components
{
    public static class ComponentRegistry
    {
        public static int MaxComponents => ComponentMask.MaxComponents;

        private static int index;

        private static Type[] idMapping;

        static ComponentRegistry()
        {
            index = 0;
            idMapping = new Type[MaxComponents];
        }


        public static int Next<T>() where T : IEntityComponent
        {
            int next = index++;
            if(idMapping.Length <= next)
                Array.Resize(ref idMapping, Mathf.NextPowerOfTwo(next));

            idMapping[next] = typeof(T);
            return next;
        }

        public static Type GetTypeForComponentID(int id)
            => (uint)id < (uint)idMapping.Length ? idMapping[id] : null;
    }
}