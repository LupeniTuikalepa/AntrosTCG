using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.UI
{
    public interface IBaseHUDPhase : IPhase
    {
        public ChannelKey ChannelKey { get; }
    }
}