namespace ATCG.Battle.Entities.Components.Premade
{
    public readonly struct ChildOfComponent : IEntityComponent
    {
        public readonly Entity entity;

        public ChildOfComponent(Entity entity)
        {
            this.entity = entity;
        }
    }
}