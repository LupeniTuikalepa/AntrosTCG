using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Status.FireStatus;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status
{
	public partial class IncandescenceStatus : Status<IncandescenceStatusData, IncandescenceComponent, StatusDurationController>, ITickOnTurnEnd
	{
		protected override IncandescenceComponent CreateStatusComponent(IncandescenceStatusData statusData, in StatusContext context)
		{
			return new IncandescenceComponent(statusData);
		}

		protected override StatusDurationController CreateStatusController(IncandescenceStatusData statusData, in StatusContext context)
		{
			return new StatusDurationController(statusData.Duration);
		}

		protected override void OnTick(IncandescenceStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(statusData, in statusInfos, in context);
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
                
					var statusCommand = new StatusApplyCommand(member.EntityAddress, statusData.Status);
					statusCommand.Run(player.BattlePhase);
				}
			}
		}

		protected override void OnStack(IncandescenceStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnStack(statusData, in statusInfos, in context);
			ref StatusDurationController controller = ref statusInfos.statusControllerRef.GetValue();
			if (controller.RemainingTicks < statusData.Duration)
			{
				controller.SetTicks(statusData.Duration);
			}
		}
	}
}