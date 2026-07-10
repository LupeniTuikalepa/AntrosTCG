using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local.Phases.Preview;
using ATCG.Battle.Players.Local.UI;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public sealed class SelectEntityPhase<T> : LocalPlayerPhase<EntityAddress[]>,
        ISelectEntityPhase, IHUDPhase<ISelectEntityPhase> where T : IEntityFilter
    {

        public event Action<ISelectEntityPhase> OnPreviewChanged;

        public event Action<EntityAddress> OnEntitySelected;
        public event Action<EntityAddress> OnEntityUnselected;
        public event Action<EntityAddress> OnEntityHovered;
        public event Action<EntityAddress> OnEntityUnhovered;

        public int MaxSelectableEntities { get; }

        public ChannelKey ChannelKey { get; private set; }

        public bool IsWaiting { get; private set; }

        private readonly CardDragPhase<IBattleCard> dragPhase;

        private HashSet<EntityAddress> selection;

        public readonly HexPatternBuilder pattern;

        private readonly T filter;

        public ISelectionPatternPreview preview;

        private List<HexCoordinates> previewedCoordinates;

        public static int PreviewSelectableQuantity(T filter, BattlePhase phase)
        {
            EntityQueryBuilder query = EntityQuery.WithFilter<T>(filter);

            int count = 0;
            World world = phase.world;
            foreach (Entity entity in world.Query(query))
            {
                if (entity.HasComponent<GridMemberComponent>(world))
                    count++;
            }

            return count;
        }

        public SelectEntityPhase(LocalBattlePlayer localBattlePlayer, T filter, HexPatternBuilder pattern,
            int maxSelectableEntities = 1) : base(localBattlePlayer)
        {
            this.filter = filter;
            this.pattern = pattern;
            MaxSelectableEntities = maxSelectableEntities;
            dragPhase = null;
        }

        public SelectEntityPhase(LocalBattlePlayer localBattlePlayer, T filter, HexPatternBuilder pattern,
            CardDragPhase<IBattleCard> dragPhase) : base(localBattlePlayer)
        {
            this.filter = filter;
            this.pattern = pattern;

            this.dragPhase = dragPhase;
            MaxSelectableEntities = 1;
        }


        public bool IsInPattern(EntityAddress address)
        {
            if (!address.TryGetComponentRO(out GridMemberComponent battleGridElement))
                return false;

            return pattern.Contains(battleGridElement.coordinates);
        }

        public bool IsRelated(EntityAddress address) => IsRelated(address, out _);
        public bool IsRelated(EntityAddress address, out EntityAddress related)
        {
            related = EntityAddress.None;

            if (!address.TryGetComponentRO(out GridMemberComponent battleGridElement))
                return false;

            if (!BattleGrid.TryGetBattleCell(battleGridElement.coordinates, out BattleCellAspect cell))
                return false;

            foreach (var member in cell.GetMembers())
            {
                if (filter.Accepts(member.EntityAddress))
                {
                    related = member.EntityAddress;
                    return true;
                }
            }

            return false;
        }

        public bool Accepts(EntityAddress address)
        {
            if (!address.HasComponent<GridMemberComponent>() || IsInPattern(address))
                return filter.Accepts(address);

            return false;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            HashSetPool<EntityAddress>.Get(out selection);
            ListPool<HexCoordinates>.Get(out previewedCoordinates);

            ChannelKey = ChannelKey.GetUniqueChannelKey();
            IsWaiting = false;

            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            HashSetPool<EntityAddress>.Release(selection);
            ListPool<HexCoordinates>.Release(previewedCoordinates);

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

                if (!Accepts(entity.Address))
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

        void IEntitySelectionController.OnSelected(IRuntimeEntity runtimeEntity, ref EntityAddress address)
        {
            if (!IsWaiting)
                return;

            if (!IsInPattern(address) || !AcceptsWithRelated(ref address))
            {
                selection.Clear();
                IsWaiting = false;
                return;
            }

            selection.Add(address);
            if (selection.Count >= MaxSelectableEntities)
                IsWaiting = false;

            OnEntitySelected?.Invoke(address);
        }

        void IEntitySelectionController.OnUnselected(IRuntimeEntity runtimeEntity, ref EntityAddress address)
        {
            if (!IsWaiting)
                return;


            if (!AcceptsWithRelated(ref address))
                return;


            selection?.Remove(address);
            OnEntityUnselected?.Invoke(address);
        }

        void IEntitySelectionController.OnHoverBegin(IRuntimeEntity runtimeEntity, ref EntityAddress address)
        {
            if (!IsWaiting)
                return;

            previewedCoordinates.Clear();
            if (AcceptsWithRelated(ref address))
            {
                OnEntityHovered?.Invoke(address);

                if (preview != null && address.TryGetComponentRO(out GridMemberComponent battleGridElement))
                {
                    HexPatternBuilder builder = preview.GetPreview(battleGridElement.coordinates);
                    previewedCoordinates.AddRange(builder.GetCoordinates());
                }
            }

            OnPreviewChanged?.Invoke(this);
        }

        void IEntitySelectionController.OnHoverEnd(IRuntimeEntity runtimeEntity, ref EntityAddress address)
        {
            if (!IsWaiting)
                return;

            previewedCoordinates.Clear();
            if (AcceptsWithRelated(ref address))
                OnEntityUnhovered?.Invoke(address);

            OnPreviewChanged?.Invoke(this);
        }

        public bool IsInPreview(EntityAddress address)
        {
            if (address.TryGetComponentRO(out GridMemberComponent gridMemberComponent))
                return IsInPreview(gridMemberComponent.coordinates);

            return false;
        }

        public bool IsInPreview(HexCoordinates coordinates) => previewedCoordinates.Contains(coordinates);

        private bool AcceptsWithRelated(ref EntityAddress address)
        {
            if (!Accepts(address))
            {
                if (IsRelated(address, out EntityAddress related))
                    address = related;
                else
                    return false;
            }

            return true;
        }
    }
}