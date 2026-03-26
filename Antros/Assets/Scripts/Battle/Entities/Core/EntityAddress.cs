namespace ATCG.Battle.Entities.Core
{
    public readonly struct EntityAddress
    {
        public readonly World world;
        public readonly Entity entity;

        public EntityAddress(World world, Entity entity)
        {
            this.world = world;
            this.entity = entity;
        }
    }
}