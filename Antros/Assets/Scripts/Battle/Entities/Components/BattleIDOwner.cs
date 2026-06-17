namespace ATCG.Battle.Entities.Components
{
    public struct BattleIDOwner : IEntityComponent
    {
        public readonly BattleID id;

        public BattleIDOwner(BattleID id)
        {
            this.id = id;
        }
    }
}