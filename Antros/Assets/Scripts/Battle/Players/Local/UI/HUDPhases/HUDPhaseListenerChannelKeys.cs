using Helteix.ChanneledProperties;

namespace ATCG.Battle.Players.Local.UI
{
    public static class HUDPhaseListenerChannelKeys<T> where T : ILocalHUDPhase
    {
        public static readonly ChannelKey ChannelKey = ChannelKey.GetUniqueChannelKey(typeof(T).Name);
    }
}