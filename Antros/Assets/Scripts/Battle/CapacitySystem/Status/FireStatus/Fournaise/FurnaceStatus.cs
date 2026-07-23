using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.FireStatus.Fournaise;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Status.FireStatus;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Fournaise
{
    public partial class FurnaceStatus : Status<FurnaceData, FurnaceComponent, StatusDurationController>, ITickOnTurnEnd
    {
       protected override FurnaceComponent CreateStatusComponent(FurnaceData data, in StatusContext context)
       {
          return new FurnaceComponent(data);
       }

       protected override StatusDurationController CreateStatusController(FurnaceData data, in StatusContext context)
       {
          return new StatusDurationController(data.Duration);
       }
       protected override void OnApply(FurnaceData data, in EntityStatusInfos statusInfos, in StatusContext context)
       {
           
       }
       
       protected override void OnTick(FurnaceData data, in EntityStatusInfos statusInfos, in StatusContext context)
       {
           base.OnTick(data, in statusInfos, in context);
           if (!statusInfos.targetAddress.TryGetComponentRO<BattleCellComponent>(out _))
	           return;

           BattleCellAspect cellAspect = new BattleCellAspect(statusInfos.targetAddress);
          
           foreach (ComponentRef<GridMemberComponent> member in cellAspect.GetMembers())
           {
	           if (!member.EntityAddress.HasComponent<HealthComponent>())
				  
		           continue;
              
	           if (member.EntityAddress.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent))
	           {
		           var player = belongsToPlayerComponent.GetPlayer(context.battlePhase);
                
		           ModifyPlayerManaCommand manaCommand = new ModifyPlayerManaCommand(player, -data.ManaRemove);
		           manaCommand.Run(context.battlePhase);

		           Debug.Log($"[Fournaise] {data.ManaRemove} mana retiré au joueur {player} !");
	           }
           }
       }

       protected override void OnRemove(FurnaceData data, in EntityStatusInfos statusInfos, in StatusContext context)
       {
           base.OnRemove(data, in statusInfos, in context);
       }
    }
}