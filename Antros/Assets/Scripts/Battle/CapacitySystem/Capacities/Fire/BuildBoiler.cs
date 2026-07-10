using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public partial struct BuildBoiler : ICapacity<BuildBoilerData>
    {
        public HexPatternBuilder GetHitPattern(BuildBoilerData data, BattleGrid battleGrid, HexCoordinates castPoint,
            HexCoordinates casterOrigin)
        {
            BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
            HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
                .With(new PointsPattern(castPoint))
                .Without(casterOrigin);

            return builder;
        }

        private partial void ExecuteBuild(BuildBoilerData data, CapacityStepContext ctx)
        {
            BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;

            using HexPatternBuilder builder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

            foreach (BattleCellAspect cellAspect in builder.GetBattleCells(battleGrid))
            {
                var spawnDeployable = new SpawnDeployableCommand(
                    ctx.BattlePhase.CurrentPlayer,
                    data.DeployableData,
                    cellAspect,
                    ctx.Caster);
                spawnDeployable.Run(ctx.BattlePhase);
            }
        }
    }
}