using System;

namespace ATCG.Battle.Entities.Queries
{
    public readonly struct EntityQueryResult
    {
        private readonly World world;
        private readonly Query query;

        public EntityQueryResult(World world, in Query query)
        {
            this.world = world;
            this.query = query;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(world, query);
        }

        public ref struct Enumerator
        {
            private readonly World world;
            private readonly Query query;
            private readonly ReadOnlySpan<int> entities;
            private readonly ReadOnlySpan<EntityMeta> metas;

            private int index;

            public Enumerator(World world, in Query query)
            {
                this.world = world;
                this.query = query;
                index = -1;
                entities = world.Entities;
                metas = world.Metas;
            }

            public bool MoveNext()
            {
                bool hasFilters = query.filters == null;
                while (++index < entities.Length)
                {
                    if (!metas[index].MatchesQuery(in query))
                        continue;

                    if (hasFilters)
                        return true;

                    bool passed = true;
                    for (int i = 0; i < query.filterCount; i++)
                    {
                        if (query.filters[i].Evaluate(Current, world))
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