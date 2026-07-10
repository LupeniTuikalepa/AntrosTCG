using ATCG.Capacities;

namespace ATCG.Battle.Entities.Components.Tags
{
    public struct DeployableEntityTag : IEntityComponent
    {
        public readonly EntityAddress caster;
        public readonly DeployableData data;

        public DeployableEntityTag(EntityAddress caster, DeployableData data)
        {
            this.caster = caster;
            this.data = data;
        }
    }
}