using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.Berserk
{
    public struct BerserkStatusComponent : IStatusComponent
    {
        public readonly ChannelKey channelKey;
        public StatusData StatusData { get; }

        public BerserkStatusComponent(BerserkStatusData data, ChannelKey channelKey)
        {
            StatusData = data;
            this.channelKey = channelKey;
        }
    }
}