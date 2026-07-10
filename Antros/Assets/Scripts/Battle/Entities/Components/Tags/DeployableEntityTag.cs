namespace ATCG.Battle.Entities.Components.Tags
{
    public struct DeployableEntityTag : IEntityComponent
    {
        public readonly EntityAddress caster;

        public DeployableEntityTag(EntityAddress caster)
        {
            this.caster = caster;
        }
    }
}