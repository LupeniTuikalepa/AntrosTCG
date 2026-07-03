using System;

namespace ATCG.Battle.Entities;

public static class EntityAspectManager<T>
{
    public static void Init(Func<EntityAddress, bool> a, Func<EntityAddress, T> b) { }
}