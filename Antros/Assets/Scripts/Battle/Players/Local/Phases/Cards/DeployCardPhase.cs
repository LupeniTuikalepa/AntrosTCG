using System;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Lookups;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.HexGrids.Utility;
using ATCG.Metrics;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.Tools.Phases;
using NUnit.Framework;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases.Cards
{
    public class DeployCardPhase : LocalPlayerPhase
    {
        private struct DeployableCellFilter : IEntityFilter
        {
            public bool Accepts(EntityAddress address)
            {
                if (address.Is(out BattleCellAspect aspect))
                    return !aspect.HasMembers;

                return false;
            }
        }


        private readonly IBattleCard battleCard;
        private readonly CardDragPhase<IBattleCard> dragPhase;

        public DeployCardPhase(LocalBattlePlayer localBattlePlayer, IBattleCard battleCard,
            CardDragPhase<IBattleCard> dragPhase) : base(localBattlePlayer)
        {
            this.battleCard = battleCard;
            this.dragPhase = dragPhase;
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            HexCoordinates center = new HexCoordinates(0, 0);

            DeployPatternController patternController = new DeployPatternController(LocalBattlePlayer.BattlePhase.BattleGrid, LocalBattlePlayer);
            DeployableCellFilter cellFilter = new DeployableCellFilter();

            using var patternBuilder = new HexPatternBuilder(center, patternController);

            LocalBattlePlayer.GetStartingLine(patternBuilder);
            GetAllDeployTarget(patternBuilder);

            SelectEntityPhase<DeployableCellFilter> selectEntityPhase = new(
                LocalBattlePlayer,
                cellFilter,
                patternBuilder,
                dragPhase);

            PhaseResult<EntityAddress[]> result = await selectEntityPhase;

            if (result.type == PhaseResultType.Cancel)
                throw new OperationCanceledException(token);

            if (result.type != PhaseResultType.Success)
                return;

            if (result.value.Length == 0)
                return;

            EntityAddress address = result.value[0];
            if (!address.Is(out GridMemberAspect aspect))
                return;

            DeployCardCommand deployCardCommand = new(battleCard, aspect.Coordinates, LocalBattlePlayer);
            await deployCardCommand.RunAsync(LocalBattlePlayer.BattlePhase);
        }

        private void GetAllDeployTarget(HexPatternBuilder patternBuilder)
        {
            World world = LocalBattlePlayer.BattlePhase.world;
            foreach (ComponentRef<DeployTargetComponent> componentRef in world.Query<DeployTargetComponent>())
            {
                EntityAddress address = componentRef.EntityAddress;
                if (!address.TryGetComponentRO(out GridMemberComponent gridMember))
                    continue;
                if (!address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer))
                    continue;
                if (!belongsToPlayer.IsAllieOf(LocalBattlePlayer))
                    continue;

                DeployTargetComponent component = componentRef.GetValue();
                patternBuilder.With(component.deployPattern, gridMember.coordinates);
            }
        }
    }
}