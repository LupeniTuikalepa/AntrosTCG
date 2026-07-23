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
	public partial class EarthQuakeStatus : Status<EarthQuakeData,EarthQuakeStatusComponent, StatusDurationController>,ITickOnTurnBegin
	{
		protected override EarthQuakeStatusComponent CreateStatusComponent(EarthQuakeData data, in StatusContext context)
		{
			return new EarthQuakeStatusComponent(data);
		}

		protected override StatusDurationController CreateStatusController(EarthQuakeData data, in StatusContext context)
		{
			return new StatusDurationController(data.Duration);
		}
		
		protected override void OnStack(EarthQuakeData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnStack(data, in statusInfos, in context);
			ref StatusDurationController controller = ref statusInfos.statusControllerRef.GetValue();
			if (controller.RemainingTicks < data.Duration)
			{
				controller.SetTicks(data.Duration);
			}
		}

		protected override void OnTick(EarthQuakeData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(data, in statusInfos, in context);
			
			if (!statusInfos.targetAddress.Is(out BattleCellAspect cellAspect))
				return;
          
			foreach (ComponentRef<GridMemberComponent> member in cellAspect.GetMembers())
			{
				if (member.EntityAddress.TryGetComponentRO(out HealthComponent memberHealth))
				{
					int damage = (memberHealth.CurrentHealth * data.DamagePercentage) / 100; 
                
					DamageCommand damageCommand = new DamageCommand(damage, member.EntityAddress);
					damageCommand.Run(context.battlePhase);
				}
			}
		}
	}
}