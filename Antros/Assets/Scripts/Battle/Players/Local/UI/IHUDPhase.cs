using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using Unity.Services.Multiplayer;

namespace ATCG.Battle.Players.Local.UI
{
    public interface IHUDPhase<T> : IHUDPhase where T : IHUDPhase
    {
        ChannelKey IHUDPhase.ChannelKey => HUDPhaseListenerChannelKeys<T>.ChannelKey;
    }

    public interface IHUDPhase : IPhase
    {
        public ChannelKey ChannelKey { get; }
    }
}