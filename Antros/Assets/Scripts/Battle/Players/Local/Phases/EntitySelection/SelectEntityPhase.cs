using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Players.Local.Phases.Cards;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public class SelectEntityPhase<T> : PhaseCompletionSource<EntityAddress>, ISelectEntityPhase
        where T : IEntityFilter
    {
        public ChannelKey ChannelKey { get; private set; }

        private readonly T filter;

        public SelectEntityPhase(T filter, CardDragPhase<IBattleCard> phase)
        {
            this.filter = filter;
        }


        public bool Accepts(EntityAddress address) => filter.Accepts(address);

        protected override async Awaitable Initialize(CancellationToken token)
        {
            ChannelKey = ChannelKey.GetUniqueChannelKey();
            await Task.CompletedTask;
        }
    }
}