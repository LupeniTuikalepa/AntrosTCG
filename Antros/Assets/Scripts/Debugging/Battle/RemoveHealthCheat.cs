using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using Cheats.Core;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
	public class RemoveHealthCheat : ICheat
	{
		public string Name { get; }
		public string Description { get; }
		public readonly RuntimeLocalBattlePlayer players;

		public RemoveHealthCheat(RuntimeLocalBattlePlayer player)
		{
			Name = nameof(RemoveHealthCheat);
			Description = "SomeBody remove your health";
			this.players = player;
		}

		public void Execute(in CheatContext context)
		{
			players.BattlePlayer.AddOrRemoveHealth(-5);
			Debug.Log(players);
			ModifyPlayerHealthCommand command = new ModifyPlayerHealthCommand(players.BattlePlayer, -5);
			command.Run(players.BattlePlayer.BattlePhase);
			Debug.Log($"[AddHealthCheat] SomeBody remove your health... now you have : {players.BattlePlayer.CurrentHealth} HP ");
		}
	}
}