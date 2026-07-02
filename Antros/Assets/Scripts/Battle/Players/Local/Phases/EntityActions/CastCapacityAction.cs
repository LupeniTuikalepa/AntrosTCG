using ATCG.Battle.Capacities;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle
{
    public class CastCapacityAction : EntityAction
    {
        public override int ManaCost => capacityData.Cost;

        public readonly CapacityData capacityData;
        private readonly HexCoordinates from;

        public CastCapacityAction(LocalBattlePlayer fromPlayer, CapacityData capacityData, HexCoordinates from) : base(fromPlayer)
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
                BattleIgnoreOriginPatternController patternController = new BattleIgnoreOriginPatternController(BattleGrid, from);
                using var patternBuilder = new HexPatternBuilder(from, patternController)
                    .With(capacityData.CastPatterns);

                AspectFilter<BattleCellAspect> filter = new AspectFilter<BattleCellAspect>();
                SelectEntityPhase<AspectFilter<BattleCellAspect>> phase =
                    new SelectEntityPhase<AspectFilter<BattleCellAspect>>(fromPlayer, filter, patternBuilder);

                EntityAddress[] result = await phase;

                for (int i = 0; i < result.Length; i++)
                {
                    EntityAddress target = result[i];
                    if (target.TryGetComponentRO(out GridMemberComponent component))
                        await ExecuteCommand(battlePhase, component.coordinates, address);
                }
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