using ATCG.Battle.Players.Local.Phases;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Unity.Services.Multiplayer;

namespace ATCG.Battle.Players.Local.UI
{
    public interface ILocalHUDPhase<T> : ILocalHUDPhase where T : ILocalHUDPhase
    {
        ChannelKey IBaseHUDPhase.ChannelKey => HUDPhaseListenerChannelKeys<T>.ChannelKey;
    }

    public interface ILocalHUDPhase : IBaseHUDPhase, ILocalPlayerPhase
    {
    }
}