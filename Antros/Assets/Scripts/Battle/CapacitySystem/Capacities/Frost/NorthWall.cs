using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Arc;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.Frost
{
    public partial struct NorthWall : ICapacity<NorthWallData>
    {
        public void GetHitPattern(NorthWallData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
        {
            builder
                .With(new ArcPattern(casterOrigin ,castPoint, data.Size))
                .Without(casterOrigin);
        }

        public void GetTargets(NorthWallData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
        {
            output.Add(battleCell.EntityAddress, CapacityTags.CELL);
            foreach (var member in battleCell.GetMembers())
                output.Add(member.EntityAddress, CapacityTags.MEMBER);
        }

        private partial void ExecuteConstruction(NorthWallData data, CapacityStepContext ctx)
        {
            foreach (var cellAddress in ctx.Targets.WithTags(CapacityTags.CELL))
            {
                if (cellAddress.Is(out BattleCellAspect cellAspect))
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
}