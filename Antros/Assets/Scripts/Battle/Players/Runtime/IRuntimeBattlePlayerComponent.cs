namespace ATCG.Battle.Players
{
    public interface IRuntimeBattlePlayerComponent
    {
        void Connect(IBattlePlayer player);
        void Disconnect(IBattlePlayer battlePlayer);
    }
}