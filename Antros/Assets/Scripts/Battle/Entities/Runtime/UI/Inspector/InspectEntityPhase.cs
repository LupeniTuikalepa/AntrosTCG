using System.Threading;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
	public class InspectEntityPhase : LocalPlayerPhase, ISinglePhase
	{
		string ISinglePhase.Channel => "EntityInspection";

		public EntityAddress EntityAddress { get; private set; }

		public IRuntimeEntity RuntimeEntity
		{
			get
			{
				if (!LocalBattlePlayer.TryGetRuntime(out var runtimeLocalBattlePlayer))
					return null;

				if(!runtimeLocalBattlePlayer.RuntimeEntityManager.TryGetRuntimeEntity(EntityAddress, out var runtimeEntity))
					return null;

				return runtimeEntity;
			}
		}

		public readonly Priority<bool> isActive;
		public InspectEntityPhase(LocalBattlePlayer localBattlePlayers, EntityAddress entityAddress) : base(localBattlePlayers)
		{
			isActive = new(false);
			EntityAddress = entityAddress;
		}

		protected override async Awaitable ExecuteNoResult(CancellationToken token)
		{
			while (isActive)
			{
				await Awaitable.EndOfFrameAsync(token);
				if (!isActive)
					await Awaitable.WaitForSecondsAsync(.2f, token);
			}
		}

	}
}