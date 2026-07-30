using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Status.FireStatus;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Furnace
{
	public partial class FurnaceStatus : Status<FurnaceStatusData, FurnaceStatusComponent, StatusDurationController>, ITickOnTurnBegin
	{
		protected override FurnaceStatusComponent CreateStatusComponent(FurnaceStatusData statusData, in StatusContext context)
		{
			return new FurnaceStatusComponent(statusData);
		}

		protected override StatusDurationController CreateStatusController(FurnaceStatusData statusData, in StatusContext context)
		{
			return new StatusDurationController(statusData.Duration);
		}

		protected override void OnApply(FurnaceStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			ref var statusComponent = ref statusInfos.statusComponentRef.GetValue();
			statusComponent.Watch(statusInfos.targetAddress);

		}

		protected override void OnTick(FurnaceStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(statusData, in statusInfos, in context);
			
			var command = TickForEntity(statusData, statusInfos.targetAddress, in context);
			command?.Run(context.battlePhase);
			
			if (!statusInfos.targetAddress.Is(out BattleCellAspect cellAspect))
				return;
			
			foreach (ComponentRef<GridMemberComponent> member in cellAspect.GetMembers())
			{
				ModifyPlayerManaCommand commandForMember = TickForEntity(statusData, member.EntityAddress, in context);
				commandForMember?.Run(context.battlePhase);
			}
		}

		protected override void OnStack(FurnaceStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnStack(statusData, in statusInfos, in context);
			ref StatusDurationController controller = ref statusInfos.statusControllerRef.GetValue();
			if (controller.RemainingTicks < statusData.Duration)
			{
				controller.SetTicks(statusData.Duration);
			}
		}

		private static ModifyPlayerManaCommand TickForEntity(FurnaceStatusData statusData, EntityAddress address,
			in StatusContext context)
		{
			if (!address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent)) 
				return null;
			
			var player = belongsToPlayerComponent.GetPlayer(context.battlePhase);
			if (player != context.battlePhase.CurrentPlayer)
				return null;

			return new ModifyPlayerManaCommand(player, -statusData.ManaRemove);

		}

		protected override void OnRemove(FurnaceStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnRemove(statusData, in statusInfos, in context);
		}
	}
}