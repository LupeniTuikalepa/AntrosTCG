namespace ATCG.Battle.Players.Runtime
{
    public interface IRuntimeBattlePlayer<out T>
    {
        public T BattlePlayer { get; }
    }
}