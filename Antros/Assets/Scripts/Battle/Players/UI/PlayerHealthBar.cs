using System;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.UI;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Players.UI
{
	[AddComponentMenu("ATCG/Gameplay/Player/UI/PlayerHealthBar")]
	public class PlayerHealthBar : BarUI, IPlayerStatUI, ICommandListener<ModifyPlayerHealthCommand> 
	{
		public IBattlePlayer BattlePlayer { get; private set; }

		
		private void OnEnable()
		{
			this.RegisterListener();
		}

		private void OnDisable()
		{
			this.UnregisterListener();
		}

		public void Connect(IBattlePlayer player)
		{
			BattlePlayer = player;

			CurrentValue = player.CurrentHealth;
			MaxValue = player.MaxHealth;

			Refresh();
		}

		public void Disconnect(IBattlePlayer player)
		{
			BattlePlayer = null;
		}

		
		public async Awaitable Play(CommandListenerState state, CommandContext context, ModifyPlayerHealthCommand command)
		{
			state.CompleteWindUp(this);

			DeltaInRangeInfos<int> infos = command.GetInfos();
			MaxValue = infos.max; 
			CurrentValue = infos.to;

			await RefreshAsync();

			state.CompleteFollowThrough(this);
			await Awaitable.MainThreadAsync();
		}
	}
}