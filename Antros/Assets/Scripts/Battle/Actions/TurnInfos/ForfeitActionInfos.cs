namespace ATCG.Battle.Actions.TurnInfos
{
    public struct ForfeitActionInfos : IBattleActionInfos
    {
        public int PlayerID { get; }

        public string ID => IBattleActionInfos.FORFEIT_ID;

        public ForfeitActionInfos(int playerID)
        {
            PlayerID = playerID;
        }
    }
}