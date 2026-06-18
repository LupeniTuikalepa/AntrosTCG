using System;
using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Local.UI;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle
{
	public class EntityPreviewListUI : PlayerHUDElement,
		IPlayerCommandListener<SpawnHeroCommand>
	{
		
		public IBattlePlayer BattlePlayer => RuntimePlayer.BattlePlayer;

		[SerializeField] private Transform container;
		[SerializeField] private EntityPreviewUI prefabEntityPreviewUI;
		
		private Dictionary<Entity, EntityPreviewUI> previews = new Dictionary<Entity, EntityPreviewUI>();

		private void Awake()
		{
			container.ClearChildren();
		}
		protected override void OnConnect()
		{
			this.RegisterListener();
		}

		protected override void OnDisconnect()
		{
			this.UnregisterListener();
		}

		async Awaitable ICommandListener<SpawnHeroCommand>.Play(CommandListenerState state, CommandContext context,
			SpawnHeroCommand command)
		{
			state.CompleteAll(this);
			if (command.SpawnID.TryGetEntityWithBattleID(context.World, out EntityAddress address))
			{
				await Awaitable.EndOfFrameAsync();
				
				EntityPreviewUI instance = prefabEntityPreviewUI.InstantiatePrefab(container);
				await instance.Connect(this, address);
				previews.Add(address, instance);
			}
		}

		public void DestroyPreview(Entity entity) => DestroyPreviewAsync(entity).ListenForExceptions();
		public async Awaitable DestroyPreviewAsync(Entity entity)
		{
			if (previews.Remove(entity, out EntityPreviewUI instance))
			{
				await instance.Disconnect(this, entity);
				instance.DestroyGameObject();
			}
		}

	}
}