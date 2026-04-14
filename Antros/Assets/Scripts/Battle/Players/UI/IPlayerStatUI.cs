namespace ATCG.Battle.Players.UI
{
    public interface IPlayerStatUI
    {
        void Connect(IBattlePlayer player);
        void Disconnect(IBattlePlayer player);
    }
}