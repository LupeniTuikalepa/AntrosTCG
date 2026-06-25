using System;
using UnityEngine;

namespace ATCG.Battle.Entities
{
    public static class EntityAspectManager<T> where T : IEntityAspect
    {
        // ReSharper disable once StaticMemberInGenericType
        private static Func<EntityAddress, bool> checkDelegate;

        // ReSharper disable once StaticMemberInGenericType
        private static Func<EntityAddress, T> getAspectDelegate;

        public static void Init(Func<EntityAddress, bool> check, Func<EntityAddress, T> getAspect)
        {
            checkDelegate = check;
            getAspectDelegate = getAspect;
        }



        public static bool TryGet(EntityAddress address, out T aspect)
        {
            if (Is(address))
            {
                aspect = getAspectDelegate(address);
                return true;
            }
            aspect = default(T);
            return false;
        }

        public static bool Is(EntityAddress address)
        {
            if (checkDelegate == null)
                return false;

            return checkDelegate(address);
        }
    }
}