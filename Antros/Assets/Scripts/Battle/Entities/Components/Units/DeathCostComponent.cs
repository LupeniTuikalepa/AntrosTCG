namespace ATCG.Battle.Entities.Components
{
    public readonly struct DeathCostComponent : IEntityComponent
    {
        public readonly int cost;

        public DeathCostComponent(int cost)
        {
            this.cost = cost;
        }
    }
}