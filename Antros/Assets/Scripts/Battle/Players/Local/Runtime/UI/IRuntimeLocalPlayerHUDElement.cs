namespace ATCG.Battle.Players.Local.Runtime.UI
{
    public interface IRuntimeLocalPlayerHUDElement
    {
        RuntimeLocalPlayerHUD HUD { get; set; }
        void Connect(LocalBattlePlayer player);
        void Disconnect(LocalBattlePlayer player);
    }
}