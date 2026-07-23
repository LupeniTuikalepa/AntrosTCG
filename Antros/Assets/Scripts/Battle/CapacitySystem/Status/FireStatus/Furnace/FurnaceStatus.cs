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
	public partial class FurnaceStatus : Status<FurnaceData, FurnaceStatusComponent, StatusDurationController>, ITickOnTurnBegin
	{
		protected override FurnaceStatusComponent CreateStatusComponent(FurnaceData data, in StatusContext context)
		{
			return new FurnaceStatusComponent(data);
		}

		protected override StatusDurationController CreateStatusController(FurnaceData data, in StatusContext context)
		{
			return new StatusDurationController(data.Duration);
		}

		protected override void OnApply(FurnaceData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			ref var statusComponent = ref statusInfos.statusComponentRef.GetValue();
			statusComponent.Watch(statusInfos.targetAddress);

		}

		protected override void OnTick(FurnaceData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(data, in statusInfos, in context);
			
			var command = TickForEntity(data, statusInfos.targetAddress, in context);
			command?.Run(context.battlePhase);
			
			if (!statusInfos.targetAddress.Is(out BattleCellAspect cellAspect))
				return;
			
			foreach (ComponentRef<GridMemberComponent> member in cellAspect.GetMembers())
			{
				var commandForMember = TickForEntity(data, member.EntityAddress, in context);
				commandForMember?.Run(context.battlePhase);
			}
		}

		protected override void OnStack(FurnaceData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnStack(data, in statusInfos, in context);
			ref StatusDurationController controller = ref statusInfos.statusControllerRef.GetValue();
			if (controller.RemainingTicks < data.Duration)
			{
				controller.SetTicks(data.Duration);
			}
		}

		private static ModifyPlayerManaCommand TickForEntity(FurnaceData data, EntityAddress address,
			in StatusContext context)
		{
			if (!address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent)) 
				return null;
			
			var player = belongsToPlayerComponent.GetPlayer(context.battlePhase);
			if (player != context.battlePhase.CurrentPlayer)
				return null;

			return new ModifyPlayerManaCommand(player, -data.ManaRemove);

		}

		protected override void OnRemove(FurnaceData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnRemove(data, in statusInfos, in context);
		}
	}
}