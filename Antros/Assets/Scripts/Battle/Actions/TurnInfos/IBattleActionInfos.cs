namespace ATCG.Battle.Actions.TurnInfos
{
    public interface IBattleActionInfos
    {
        public const string FORFEIT_ID = "FORFEIT";
        public const string INVOCATION_ID = "INVOCATION";
        public const string MOVE_ID = "INVOCATION";
        public const string ABILITY_ID = "ABILITY";

        int PlayerID { get; }

        string ID { get; }
    }
}