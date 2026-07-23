using ATCG.Battle.CapacitySystem.Status.FireStatus.Incandescence;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Status.FireStatus;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status
{
	public partial class IncandescenceStatus : Status<IncandescenceData, IncandescenceComponent, StatusDurationController>, ITickOnTurnEnd
	{
		protected override IncandescenceComponent CreateStatusComponent(IncandescenceData data, in StatusContext context)
		{
			return new IncandescenceComponent(data);
		}

		protected override StatusDurationController CreateStatusController(IncandescenceData data, in StatusContext context)
		{
			return new StatusDurationController(data.Duration);
		}

		protected override void OnTick(IncandescenceData data, in EntityStatusInfos statusInfos, in StatusContext context)
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
                
					var statusCommand = new StatusApplyCommand(member.EntityAddress, data.Status);
					statusCommand.Run(player.BattlePhase);
				}
			}
		}
	}
}