using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.PassiveSystem.Core;
using ATCG.Capacities.Frost;
using ATCG.HexGrids.Utility;
using ATCG.Passives.Datas.Datas;

namespace ATCG.Battle.PassiveSystem.Passives
{
    public partial struct ExtendedDestruction : IPassive<ExtendedDestructionData>
    {
        public const string DEAD_ENTITY = "deadIceWall";
        public IEnumerable<IPassiveCommandListener> GetListeners(ExtendedDestructionData data, EntityAddress target)
        {
            yield return new PassiveCommandListener<DeathCommand>(data, target)
            {
                accepts = IsIceWall,
                setupContext = (context, commandContext, command) =>
                {
                    context.AddProperty(DEAD_ENTITY, command.TargetEntityAddress(commandContext.World));
                }
            };
        }

        public void Tick(ExtendedDestructionData data, PassiveContext ctx)
        {
            if (!ctx.TryGet(DEAD_ENTITY, out EntityAddress target))
                return;
            
            if (target.TryGetComponentRO(out GridMemberComponent gridMember))
            {
                var from = gridMember.coordinates;
                var battleGrid = gridMember.grid;
                var battlePhase = battleGrid.battlePhase;

                foreach (var direction in HexOperations.Directions)
                {
                    var coord = from + direction;
                    if (!battleGrid.TryGetBattleCell(coord, out var neighbor)) 
                        continue;

                    foreach (var member in neighbor.GetMembers())
                    {
                        var memberEntityAddress = member.EntityAddress;
                        if (memberEntityAddress.Is<DeployableAspect>(out var deployable))
                        {
                            var deployableData = deployable.DeployableEntityTag.data;

                            if (deployableData is not IceWallData) 
                                continue;

                            var deathCommand = new DeathCommand(memberEntityAddress);
                            deathCommand.Run(battlePhase);
                        }
                    }
                }
            }
        }

        private static bool IsIceWall(CommandContext ctx, DeathCommand command)
        {
            if (!command.TargetEntityAddress(ctx.World).Is<DeployableAspect>(out var deployable)) 
                return false;

            if (deployable.DeployableEntityTag.data is not IceWallData iceWallData) 
                return false;

            return true;
        }
    }
}