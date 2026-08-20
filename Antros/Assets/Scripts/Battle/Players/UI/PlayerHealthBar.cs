using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
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
	public class PlayerHealthBar : BarUI, IPlayerStatUI, ICommandDirector<ModifyPlayerHealthCommand>
	{
		public IBattlePlayer BattlePlayer { get; private set; }


		private void OnEnable()
		{
			this.Register();
		}

		private void OnDisable()
		{
			this.Unregister();
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


		public async Awaitable Play(CommandDirectorState state, CommandContext context, ModifyPlayerHealthCommand command)
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