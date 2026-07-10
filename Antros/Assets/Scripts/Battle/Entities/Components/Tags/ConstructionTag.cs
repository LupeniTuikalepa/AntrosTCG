using ATCG.Construction;

namespace ATCG.Battle.Entities.Components.Tags
{
    public struct ConstructionTag : IEntityComponent
    {
        private readonly ConstructionData data;

        public int AffectRange => data.AffectRange;
        public ConstructionTag(ConstructionData data)
        {
            this.data = data;
        }
    }
}