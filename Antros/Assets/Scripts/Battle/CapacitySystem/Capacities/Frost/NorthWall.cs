using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Arc;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct NorthWall : ICapacity<NorthWallData>
    {
        public HexPatternBuilder GetHitPattern(NorthWallData data, BattleGrid battleGrid, HexCoordinates castPoint,
            HexCoordinates casterOrigin)
        {
            BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
            HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
                .With(new ArcPattern(casterOrigin ,castPoint, data.Radius))
                .Without(casterOrigin);

            return builder;
        }

        private partial void ExecuteConstruction(NorthWallData data, CapacityStepContext ctx)
        {
            BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;

            using HexPatternBuilder builder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

            foreach (BattleCellAspect cellAspect in builder.GetBattleCells(battleGrid))
            {
                var spawnDeployable = new SpawnDeployableCommand(ctx.BattlePhase.CurrentPlayer ,data.DeployableData, cellAspect);
                spawnDeployable.Run(ctx.BattlePhase);
            }
        }
    }
}