namespace ATCG.Battle.Players.Runtime
{
    public interface IRuntimeBattlePlayerComponent<in T> where T : IBattlePlayer
    {
        void Connect(RuntimeBattlePlayer runtimeBattlePlayer, T player);
        void Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, T battlePlayer);
    }
}