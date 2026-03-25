using ATCG.Battle.Entities.Core.Components;

namespace ATCG.Battle.Entities.Core
{
    public struct EntityMeta
    {
        public bool isActive;

        private ComponentMask componentMask;

        public void AddComponentToMask<T>() where T : struct, IEntityComponent
            => componentMask.Set(ComponentID<T>.ID);

        public void RemoveComponentFromMask<T>() where T : struct, IEntityComponent
            => componentMask.Clear(ComponentID<T>.ID);

        public readonly bool HasAnyComponents(in ComponentMask mask)
            => componentMask.MatchesAny(mask);

        public readonly bool HasComponents(in ComponentMask mask)
            => componentMask.MatchesAll(mask);

        public readonly bool HasComponent<T>() where T : struct, IEntityComponent
            => componentMask.Has(ComponentID<T>.ID);
    }
}