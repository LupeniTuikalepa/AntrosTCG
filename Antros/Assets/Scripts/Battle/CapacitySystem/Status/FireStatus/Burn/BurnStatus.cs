using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using UnityEngine;

namespace ATCG.Battle
{
	public partial class BurnStatus : Status<BurnStatusData, StatusDurationController>,
		ITickOnTurnBegin
	{
		protected override StatusDurationController CreateStatusController(BurnStatusData data,
			in StatusContext context)
		{
			return new StatusDurationController(data.normalDuration);
		}


		protected override void OnStack(BurnStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			statusInfos.StatusController.AddOrRemoveTicks(data.normalDuration);
			base.OnStack(data, in statusInfos, in context);
		}

		protected override void OnTick(BurnStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(data, in statusInfos, context);
			int damage = data.Damage * statusInfos.StatusController.RemainingTicks;

			if (statusInfos.targetAddress.HasComponent<HealthComponent>())
			{
				Debug.Log(damage + " damage sur " + statusInfos.targetAddress);
				DamageCommand damageCommand = new DamageCommand(damage, statusInfos.targetAddress);
				damageCommand.Run(context.battlePhase);
			}

			if (!statusInfos.targetAddress.Is(out BattleCellAspect cellAspect))
				return;

			foreach (ComponentRef<GridMemberComponent> member in cellAspect.GetMembers())
			{
				if (!member.EntityAddress.HasComponent<HealthComponent>())
					continue;

				DamageCommand damageCommand = new DamageCommand(damage, member.EntityAddress);
				damageCommand.Run(context.battlePhase);

				Debug.Log(
					$"[BurnStatus] {damage} dégâts de zone infligés à l'entité {member.EntityAddress} sur la case !");
			}
		}
	}
}