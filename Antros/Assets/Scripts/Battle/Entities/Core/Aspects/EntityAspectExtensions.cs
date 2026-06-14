namespace ATCG.Battle.Entities
{
    public static class EntityAspectExtensions
    {

        public static Entity ToAddress<T>(this T aspect) where T : IEntityAspect
            => aspect.EntityAddress;

        public static Entity ToEntity<T>(this T aspect) where T : IEntityAspect
            => ToAddress(aspect);

        public static bool IsValid<T>(this T aspect) where T : IEntityAspect
        {
            return aspect.IsValid;
        }

        public static ComponentMask GetMask<T>(this T aspect) where T : IEntityAspect
        {
            return aspect.ComponentMask;
        }


        public static bool IsNot<T>(this T aspect, T other) where T : IEntityAspect
        {
            return !aspect.Is(other);
        }

        public static bool Is<T>(this EntityAddress address) where T : IEntityAspect => EntityAspectManager<T>.Is(address);

        public static bool Is<T>(this EntityAddress address, out T aspect) where T : IEntityAspect => EntityAspectManager<T>.TryGet(address, out aspect);

        public static bool Is<T>(this T aspect, T other) where T : IEntityAspect
        {
            return aspect.EntityAddress == other.EntityAddress;
        }
    }
}