using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Status.FireStatus;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.FireStatus.Fury
{
	public partial class FuryStatus : Status<PyroFuryData, FuryComponent, StatusDurationController>
	{
		private int finalBuff = 1;

		protected override FuryComponent CreateStatusComponent(PyroFuryData data, in StatusContext context)
		{
			return new FuryComponent(data, ChannelKey.GetUniqueChannelKey("PyroFury"));
		}

		protected override StatusDurationController CreateStatusController(PyroFuryData data, in StatusContext context)
		{
			return new StatusDurationController(data.Duration);
		}

		protected override void OnApply(PyroFuryData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			if (statusInfos.targetAddress.HasStatusWithData<BurnStatusData>(out var tag))
			{
				if (tag.EntityAddress.TryGetComponentRO(out StatusDurationController durationController))
					finalBuff = durationController.RemainingTicks;

				if (statusInfos.targetAddress.TryGetComponentRO(out BasicAttackerComponent basicAttackerComponent))
				{
					ChannelKey channelKey = statusInfos.StatusComponent.channelKey;
					basicAttackerComponent.strength.Multiply(channelKey, finalBuff);
				}
			}
			else
				Debug.Log("N'a pas de Flame sur lui donc aucun buff");
		}

		protected override void OnRemove(PyroFuryData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			if (statusInfos.targetAddress.TryGetComponentRO(out BasicAttackerComponent basicAttackerComponent))
			{
				ChannelKey channelKey = statusInfos.StatusComponent.channelKey;
				basicAttackerComponent.strength.RemoveOperation(channelKey);
			}
			base.OnRemove(data, in statusInfos, in context);
		}
	}
}