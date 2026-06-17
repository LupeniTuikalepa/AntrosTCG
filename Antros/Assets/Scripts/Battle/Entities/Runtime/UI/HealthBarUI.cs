using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime;
using ATCG.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle
{
	public class HealthBarUI : BarUI, IEntityCommandListener<DamageCommand>
	{
		private IRuntimeEntity runtimeEntity;
		public Entity Entity => runtimeEntity.Address.entity;

		private void OnEnable()
		{
			this.RegisterPlayer();
		}

		private void OnDisable()
		{
			this.UnregisterPlayer();
		}

		private void Start()
		{
			runtimeEntity = GetComponentInParent<IRuntimeEntity>();
		}

		public async Awaitable Play(CommandListenerState state, CommandContext context, DamageCommand command)
		{
			await Awaitable.MainThreadAsync();

			state.CompleteWindUp(this);
			state.CompleteFollowThrough(this);

			var info = command.GetInfos();
			CurrentValue = info.currentHealth;
			MaxValue = info.maxHealth;

			Refresh();
		}

	}
}