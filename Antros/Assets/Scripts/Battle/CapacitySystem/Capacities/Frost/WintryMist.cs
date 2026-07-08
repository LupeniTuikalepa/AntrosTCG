using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct WintryMist : ICapacity<WintryMistData>
    {
        public HexPatternBuilder GetHitPattern(WintryMistData data, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
            HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
                .With(new LinePattern(casterOrigin))
                .Without(casterOrigin);

            return builder;
        }

        private partial void ExecuteBlackIce(WintryMistData data, CapacityStepContext ctx)
        {
            BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;

            using HexPatternBuilder builder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

            foreach (BattleCellAspect cellAspect in builder.GetBattleCells(battleGrid))
            {
                var statusCommand = new StatusApplyCommand(cellAspect.EntityAddress, data.Status);
                statusCommand.Run(ctx.BattlePhase);
            }
        }
    }
}