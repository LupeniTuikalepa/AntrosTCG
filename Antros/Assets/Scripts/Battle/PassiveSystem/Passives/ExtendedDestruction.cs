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
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.PassiveSystem.Passives
{
    public readonly partial struct ExtendedDestruction : IPassive<ExtendedDestructionData>
    {
        public const string DEAD_ENTITY = "deadIceWall";
        public const string DEATH_CAUSE = nameof(ExtendedDestruction);

        public IEnumerable<IPassiveCommandListener> GetListeners(ExtendedDestructionData data ,PassiveContext ctx)
        {
            yield return new PassiveCommandListener<DeathCommand>(data, ctx.owner)
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
            using (HashSetPool<EntityAddress>.Get(out var deadDeployables))
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

                                if (deadDeployables.Add(memberEntityAddress)) ;
                            }
                        }
                    }

                    foreach (var deployable in deadDeployables)
                    {
                        var deathCommand = new DeathCommand(deployable, DEATH_CAUSE);
                        deathCommand.Run(battlePhase);
                    }
                }
            }
        }

        public void Apply(ExtendedDestructionData data, PassiveContext ctx)
        {
        }

        public void Remove(ExtendedDestructionData data, PassiveContext ctx)
        {
        }

        private static bool IsIceWall(CommandContext ctx, DeathCommand command)
        {
            Debug.Log($"Checking Ice Wall");
            if(command.Source == DEATH_CAUSE)
                return false;
            
            if (!command.TargetEntityAddress(ctx.World).Is<DeployableAspect>(out var deployable)) 
                return false;

            if (deployable.DeployableEntityTag.data is not IceWallData iceWallData) 
                return false;

            return true;
        }
    }
}