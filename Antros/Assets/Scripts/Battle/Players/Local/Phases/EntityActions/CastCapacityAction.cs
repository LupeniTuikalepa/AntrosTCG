using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.Phases.Preview;
using ATCG.Capacities;
using ATCG.Metrics;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle
{
    public class CastCapacityAction : EntityAction
    {
        public override int ManaCost => capacityData.Cost;

        public readonly CapacityData capacityData;
        private readonly HexCoordinates from;


        private sealed class CapacityHitPreview : ISelectionPreviewController
        {
            private readonly CapacityData data;
            private readonly BattleGrid battleGrid;
            private readonly HexCoordinates from;
            private readonly IBattlePlayer castingPlayer;

            private HexPatternBuilder hexPatternBuilder;

            public CapacityHitPreview(CapacityData data, BattleGrid battleGrid, HexCoordinates from, IBattlePlayer castingPlayer)
            {
                this.data = data;
                this.battleGrid = battleGrid;
                this.from = from;
                this.castingPlayer = castingPlayer;
            }

            public HexPatternBuilder GetPreview(HexCoordinates coordinates)
            {
                HexPatternBuilder builder = new HexPatternBuilder(coordinates, new BattleIgnoreOriginPatternController(battleGrid, coordinates));
                if (data.TryGet(out ICapacityContainer container))
                    container.GetHitPattern(data, ref builder, battleGrid, coordinates, from);

                return builder;
            }

            public void FillPreview(ISelectEntityPhase phase, EntityAddress entityAddress, List<EntityAddress> previews)
            {
                if (entityAddress.TryGetComponentRO(out GridMemberComponent memberComponent))
                {
                    if (data.TryGet(out ICapacityContainer container))
                    {
                        HexPatternBuilder builder = new HexPatternBuilder(memberComponent.coordinates, new BattleIgnoreOriginPatternController(battleGrid, memberComponent.coordinates));
                        container.GetHitPattern(data, ref builder, battleGrid, memberComponent.coordinates, from);
                        using HexPatternBuilder _hp = builder;

                        CapacityTargets targets = new CapacityTargets();
                        foreach (var battleCellAspect in builder.GetBattleCells(battleGrid))
                            container.GetTargets(data, battleCellAspect, targets, castingPlayer);

                        foreach (EntityAddress target in targets)
                            previews.Add(target);
                    }
                }
            }
        }

        public CastCapacityAction(LocalBattlePlayer fromPlayer, CapacityData capacityData, HexCoordinates from) :
            base(fromPlayer)
        {
            this.capacityData = capacityData;
            this.from = from;
        }

        public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            var patterns = capacityData.CastPatterns;

            if (patterns.IsEmpty)
            {
                if (address.TryGetComponentRO(out GridMemberComponent component))
                    await ExecuteCommand(battlePhase, component.coordinates, address);
            }
            else
            {
                if(!capacityData.TryGet(out ICapacityContainer container))
                    return;

                var patternController =
                    container.GetController(capacityData, BattleGrid, from);
                
                var patternBuilder = new HexPatternBuilder(from, patternController)
                    .With(capacityData.CastPatterns);
                
                container.ModifyCastPattern(capacityData, ref patternBuilder, BattleGrid);

                AspectFilter<BattleCellAspect> filter = new AspectFilter<BattleCellAspect>();
                SelectEntityPhase<AspectFilter<BattleCellAspect>> phase =
                    new SelectEntityPhase<AspectFilter<BattleCellAspect>>(fromPlayer, filter, patternBuilder)
                    {
                        previewController = new CapacityHitPreview(capacityData, BattleGrid, from, fromPlayer),
                        HighlightTheme = GameMetrics.Current.HighlightSettings != null
                            ? GameMetrics.Current.HighlightSettings.CastTheme
                            : null,
                    };

                EntityAddress[] result = await phase;

                for (int i = 0; i < result.Length; i++)
                {
                    EntityAddress target = result[i];
                    if (target.TryGetComponentRO(out GridMemberComponent component))
                        await ExecuteCommand(battlePhase, component.coordinates, address);
                }
                
                patternBuilder.Dispose();
            }
        }

        private async Awaitable ExecuteCommand(BattlePhase battlePhase, HexCoordinates source, EntityAddress caster)
        {
            // Routing key for the directors: the caster's owning player. Derived
            // from the entity here; for spell cards (no entity) the other setup
            // ctor takes the player id directly. Identity comes from the
            // component, not from iteration order.
            BattleID casterPlayerId = caster.TryGetComponentRO(out BelongsToPlayerComponent owner)
                ? owner.playerId
                : fromPlayer.ID;

            CapacitySetup setup = new CapacitySetup(capacityData, source, battlePhase, caster, casterPlayerId);
            await CapacityManager.CastCapacityAsync(capacityData, setup);
        }
    }
}