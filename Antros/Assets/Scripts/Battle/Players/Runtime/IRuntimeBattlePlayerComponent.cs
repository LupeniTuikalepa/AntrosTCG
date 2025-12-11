namespace ATCG.Battle.Players.Runtime
{
    public interface IRuntimeBattlePlayerComponent
    {
        void Connect(IBattlePlayer player);
        void Disconnect(IBattlePlayer battlePlayer);
    }
}