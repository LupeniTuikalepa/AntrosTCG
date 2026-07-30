using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct WintryMist : ICapacity<WintryMistData>
    {
        public void GetHitPattern(WintryMistData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            builder = builder
                .With(new LinePattern(casterOrigin))
                .Without(casterOrigin);

        }

        public void GetTargets(WintryMistData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
        {
            output.Add(battleCell.EntityAddress, CapacityTags.CELL);
        }

        private partial void ExecuteBlackIce(WintryMistData data, CapacityStepContext ctx)
        {
            foreach (EntityAddress cell in ctx.Targets.WithTags(CapacityTags.CELL))
            {
                if (cell.Is<BattleCellAspect>(out var cellAspect))
                {
                    var statusCommand = new StatusApplyCommand(cellAspect.EntityAddress, data.Status);
                    statusCommand.Run(ctx.BattlePhase);
                }
            }
        }
    }
}