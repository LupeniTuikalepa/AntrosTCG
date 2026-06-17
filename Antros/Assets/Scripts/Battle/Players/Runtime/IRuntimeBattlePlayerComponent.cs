namespace ATCG.Battle.Players.Runtime
{
    public interface IRuntimeBattlePlayerComponent<in T> where T : IBattlePlayer
    {
        void Connect(IRuntimeBattlePlayer<T> runtimeBattlePlayer);
        void Disconnect(IRuntimeBattlePlayer<T> runtimeBattlePlayer);
    }
}