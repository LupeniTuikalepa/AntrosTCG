namespace ATCG.Battle.Players.Runtime.Local.UI.HUD
{
    public interface IRuntimeLocalHUDElement
    {
        RuntimeLocalHUD HUD { set; }
        void Connect(RuntimeLocalBattlePlayer runtimePlayer, LocalBattlePlayer player);
        void Disconnect(RuntimeLocalBattlePlayer runtimePlayer, LocalBattlePlayer player);
    }
}