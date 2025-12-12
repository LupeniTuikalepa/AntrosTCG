namespace ATCG.Battle.Players.Runtime.UI
{
    public interface IPlayerStatUI
    {
        void Connect(IBattlePlayer player);
        void Disconnect(IBattlePlayer player);
    }
}