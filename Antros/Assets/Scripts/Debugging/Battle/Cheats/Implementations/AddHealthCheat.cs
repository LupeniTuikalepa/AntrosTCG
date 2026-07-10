using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local.Runtime;
using Cheats.Core;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
	public class AddHealthCheat : ICheat
	{
		public string Name { get; }
		public string Description { get; }

		private readonly RuntimeLocalBattlePlayer players;

		public AddHealthCheat(RuntimeLocalBattlePlayer player)
		{
			Name = "Heal";
			Description = "Somebody give you some heal";
			players = player;
		}

		public async Awaitable Execute(CheatContext context)
		{
			await Awaitable.MainThreadAsync();
			
			players.BattlePlayer.AddOrRemoveHealth(20);
			Debug.Log(players);
			ModifyPlayerHealthCommand command = new ModifyPlayerHealthCommand(players.BattlePlayer, 20);
			command.Run(players.BattlePlayer.BattlePhase);
			Debug.Log($"[AddHealthCheat] Somebody give you some heal... now you have : {players.BattlePlayer.CurrentHealth} HP ");
		}
	}
}