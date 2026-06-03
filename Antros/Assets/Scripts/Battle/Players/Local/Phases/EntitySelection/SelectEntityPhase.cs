using System.Threading;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
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

        private readonly CardDragPhase<IBattleCard> dragPhase;

        private readonly T filter;

        public SelectEntityPhase(T filter, CardDragPhase<IBattleCard> dragPhase)
        {
            this.filter = filter;
            this.dragPhase = dragPhase;
        }


        public bool Accepts(EntityAddress address) => filter.Accepts(address);

        protected override async Awaitable Initialize(CancellationToken token)
        {
            ChannelKey = ChannelKey.GetUniqueChannelKey();
            await base.Initialize(token);

            if (dragPhase != null)
                _ = WaitForSelection(token);
        }

        private async Awaitable WaitForSelection(CancellationToken token)
        {
            PhaseResult<DragResult<IBattleCard>> result = await dragPhase.WaitAsync(token);

            if(!IsRunning())
                return;

            if (result is { type: PhaseResultType.Success, value: { Target: IRuntimeEntity entity } })
            {
                if(Accepts(entity.Address))
                    SetResult(entity.Address);
            }
            else
            {
                SetCanceled();
            }
        }
    }
}