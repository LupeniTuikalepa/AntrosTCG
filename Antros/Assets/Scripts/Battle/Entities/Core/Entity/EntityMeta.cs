using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;

namespace ATCG.Battle.Entities
{
    public struct EntityMeta
    {
        public bool isActive;

        private ComponentMask componentMask;


        public readonly bool MatchesQuery(in Query query)
        {
            return query.Matches(componentMask);
        }

        public void AddComponentToMask<T>() where T : struct, IEntityComponent
        {
            componentMask.Set(ComponentID<T>.ID);
        }

        public void RemoveComponentFromMask<T>() where T : struct, IEntityComponent
        {
            componentMask.Clear(ComponentID<T>.ID);
        }

        public readonly bool HasAnyComponents(in ComponentMask mask)
        {
            return componentMask.MatchesAny(mask);
        }

        public readonly bool HasAllComponents(in ComponentMask mask)
        {
            return componentMask.MatchesAll(mask);
        }

        public readonly bool HasComponent<T>() where T : struct, IEntityComponent
        {
            return componentMask.Has(ComponentID<T>.ID);
        }
    }
}