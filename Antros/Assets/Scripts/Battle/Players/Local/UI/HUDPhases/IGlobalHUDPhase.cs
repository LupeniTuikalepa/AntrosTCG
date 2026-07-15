using Helteix.ChanneledProperties;

namespace ATCG.Battle.Players.Local.UI
{
    public interface IGlobalHUDPhase<T> : IGlobalHUDPhase where T : ILocalHUDPhase
    {
        ChannelKey IBaseHUDPhase.ChannelKey => HUDPhaseListenerChannelKeys<T>.ChannelKey;
    }

    public interface IGlobalHUDPhase : IBaseHUDPhase
    {
    }
}