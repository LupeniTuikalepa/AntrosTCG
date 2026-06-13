using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public sealed class SelectEntityPhase<T> : Phase<EntityAddress[]>,
        ISelectEntityPhase
        where T : IEntityFilter
    {
        public int MaxSelectableEntities { get; }
        public ChannelKey ChannelKey { get; private set; }

        public bool IsWaiting { get; private set; }

        private readonly CardDragPhase<IBattleCard> dragPhase;

        private readonly T filter;

        private HashSet<EntityAddress> selection;


        public static int PreviewSelectableQuantity(T filter, BattlePhase phase)
        {
            EntityQueryBuilder query = EntityQuery.WithFilter(filter);

            int count = 0;
            World world = phase.world;
            foreach (Entity entity in world.Query(query))
            {
                if(entity.HasComponent<GridMemberComponent>(world))
                    count++;
            }

            return count;
        }

        public SelectEntityPhase(T filter, int maxSelectableEntities = 1)
        {
            this.filter = filter;
            MaxSelectableEntities = maxSelectableEntities;
            dragPhase = null;
        }

        public SelectEntityPhase(T filter, CardDragPhase<IBattleCard> dragPhase)
        {
            this.filter = filter;
            this.dragPhase = dragPhase;

            MaxSelectableEntities = 1;
        }

        public bool Accepts(EntityAddress address) => filter.Accepts(address);

        protected override Awaitable Initialize(CancellationToken token)
        {
            HashSetPool<EntityAddress>.Get(out selection);
            ChannelKey = ChannelKey.GetUniqueChannelKey();
            IsWaiting = false;

            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            HashSetPool<EntityAddress>.Release(selection);
            selection = null;
            IsWaiting = false;
            return base.Dispose(token);
        }

        protected override async Awaitable<EntityAddress[]> Execute(CancellationToken token)
        {
            if (dragPhase != null)
            {
                PhaseResult<DragResult<IBattleCard>> result = await dragPhase.WaitAsync(token);

                if (!IsRunning())
                    return Array.Empty<EntityAddress>();

                if (result is not { type: PhaseResultType.Success, value: { Target: IRuntimeEntity entity } })
                    return Array.Empty<EntityAddress>();

                if(!Accepts(entity.Address))
                    return Array.Empty<EntityAddress>();

                return new[] { entity.Address };

            }

            IsWaiting = true;
            while (IsWaiting)
            {
                token.ThrowIfCancellationRequested();
                await Awaitable.NextFrameAsync(token);
            }

            return selection.ToArray();
        }

        public void ValidateCurrentSelection() => IsWaiting = false;

        public void ClearSelection() => selection.Clear();

        void IEntitySelectionController.OnSelected(IRuntimeEntity runtimeEntity)
        {
            if(!IsWaiting)
                return;

            selection?.Add(runtimeEntity.Address);
            if(selection != null && selection.Count >= MaxSelectableEntities)
                IsWaiting = false;
        }

        void IEntitySelectionController.OnUnselected(IRuntimeEntity runtimeEntity)
        {
            if(!IsWaiting)
                return;

            selection?.Remove(runtimeEntity.Address);
        }
    }
}