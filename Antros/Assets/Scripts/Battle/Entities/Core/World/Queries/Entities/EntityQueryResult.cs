using System;

namespace ATCG.Battle.Entities.Queries
{
    // SYNC: Any operations added here must also be added to ComponentQuery<T, TFilter>.
    // Both types intentionally share the same query operation API but cannot
    // share code due to ref struct Enumerator constraints.
    public readonly struct EntityQueryResult
    {
        private readonly World world;
        private readonly EntityQuery entityQuery;

        public EntityQueryResult(World world, in EntityQuery entityQuery)
        {
            this.world = world;
            this.entityQuery = entityQuery;
        }

        public Enumerator GetEnumerator() => new Enumerator(world, entityQuery);

        /// <summary>Returns true if at least one entity matches the query.</summary>
        public bool Any()
        {
            foreach (var _ in this)
                return true;
            return false;
        }

        /// <summary>Returns the number of entities matching the query.</summary>
        public int Count()
        {
            int count = 0;
            foreach (var _ in this)
                count++;
            return count;
        }

        /// <summary>Returns the first matching entity, or false if none.</summary>
        public bool TryGetFirst(out Entity entity)
        {
            foreach (var e in this)
            {
                entity = e;
                return true;
            }
            entity = default;
            return false;
        }

        /// <summary>
        /// Returns the single matching entity.
        /// Returns false if there are none or more than one match.
        /// </summary>
        public bool TryGetSingle(out Entity entity)
        {
            entity = default;
            bool found = false;
            foreach (var e in this)
            {
                if (found)
                {
                    entity = default;
                    return false;
                }
                entity = e;
                found = true;
            }
            return found;
        }

        public ref struct Enumerator
        {
            private readonly World world;
            private readonly EntityQuery entityQuery;
            private readonly ReadOnlySpan<int> entities;
            private readonly ReadOnlySpan<EntityMeta> metas;

            private int index;

            public Enumerator(World world, in EntityQuery entityQuery)
            {
                this.world = world;
                this.entityQuery = entityQuery;
                index = -1;
                entities = world.Entities;
                metas = world.Metas;
            }

            public bool MoveNext()
            {
                bool hasNoFilters = entityQuery.filters == null;
                while (++index < entities.Length)
                {
                    if (!metas[index].MatchesQuery(in entityQuery))
                        continue;

                    if (hasNoFilters)
                        return true;

                    bool passed = true;
                    for (int i = 0; i < entityQuery.filterCount; i++)
                    {
                        if (entityQuery.filters[i].Accepts(new EntityAddress(world, Current)))
                            continue;

                        passed = false;
                        break;
                    }

                    if (!passed)
                        continue;

                    return true;
                }

                return false;
            }

            public Entity Current => new(entities[index]);
        }
    }
}