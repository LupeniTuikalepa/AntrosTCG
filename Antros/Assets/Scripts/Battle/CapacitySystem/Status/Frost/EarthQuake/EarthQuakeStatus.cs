using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Frost.EarthQuake
{
	public partial class EarthQuakeStatus : Status<EarthQuakeStatusData,EarthQuakeStatusComponent, StatusDurationController>,ITickOnTurnBegin
	{
		protected override EarthQuakeStatusComponent CreateStatusComponent(EarthQuakeStatusData statusData, in StatusContext context)
		{
			return new EarthQuakeStatusComponent(statusData);
		}

		protected override StatusDurationController CreateStatusController(EarthQuakeStatusData statusData, in StatusContext context)
		{
			return new StatusDurationController(statusData.Duration);
		}
		
		protected override void OnStack(EarthQuakeStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnStack(statusData, in statusInfos, in context);
			ref StatusDurationController controller = ref statusInfos.statusControllerRef.GetValue();
			if (controller.RemainingTicks < statusData.Duration)
			{
				controller.SetTicks(statusData.Duration);
			}
		}

		protected override void OnTick(EarthQuakeStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(statusData, in statusInfos, in context);
			
			if (!statusInfos.targetAddress.Is(out BattleCellAspect cellAspect))
				return;
          
			foreach (ComponentRef<GridMemberComponent> member in cellAspect.GetMembers())
			{
				if (member.EntityAddress.TryGetComponentRO(out HealthComponent memberHealth))
				{
					int damage = (memberHealth.CurrentHealth * statusData.DamagePercentage) / 100; 
                
					DamageCommand damageCommand = new DamageCommand(damage, member.EntityAddress);
					damageCommand.Run(context.battlePhase);
				}
			}
		}
	}
}