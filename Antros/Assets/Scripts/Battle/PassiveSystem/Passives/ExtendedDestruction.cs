using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.PassiveSystem.Core;
using ATCG.Battle.Players;
using ATCG.Capacities.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.PassiveSystem.Passives
{
    public class ExtendedDestruction : Passive<DamageCommand>
    {
        private EntityAddress targetEntityAddress;
        private DeltaInRangeInfos<int> commandInfos;
        
        /*
            var battleGrid = context.Grid;
            
            if(!targetEntityAddress.TryGetComponentRO<GridMemberComponent>(out var gridMember))
                return;
                
            if(!battleGrid.TryGetBattleCell(gridMember.coordinates, out var cell))
                return;
            
            DestroyNearbyWall(context, battleGrid, cell.Coordinate);
        */

        private void DestroyNearbyWall(CommandContext context, BattleGrid battleGrid, HexCoordinates from)
        {
            foreach (var direction in HexOperations.Directions)
            {
                var coord = from + direction;
                if(!battleGrid.TryGetBattleCell(coord, out var neighbor))
                    continue;

                foreach (var member in neighbor.GetMembers())
                {
                    var memberEntityAddress = member.EntityAddress;
                    if (memberEntityAddress.Is<DeployableAspect>(out var deployable))
                    {
                        var deployableData = deployable.DeployableEntityTag.data;
                        
                        if (deployableData is not IceWallData)
                            continue;
                        
                        var damageCommand = new DamageCommand(99, memberEntityAddress);
                        damageCommand.Run(context.battlePhase);
                        DestroyNearbyWall(context, battleGrid, coord);
                    }
                }
            }
        }

        public override bool Accepts(CommandContext context, DamageCommand command)
        {
            targetEntityAddress = command.TargetEntityAddress(context.World);
            commandInfos = command.GetInfos();

            if (!targetEntityAddress.Is<DeployableAspect>(out var deployable)) 
                return false;

            if (deployable.DeployableEntityTag.data is not IceWallData) 
                return false;

            return true;
        }
    }
}