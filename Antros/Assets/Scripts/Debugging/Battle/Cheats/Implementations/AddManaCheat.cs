using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local.Runtime;
using Cheats.Core;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
	public class AddManaCheat : ICheat
	{
		public string Name { get; }
		public string Description { get; }
		public readonly RuntimeLocalBattlePlayer players;

		public AddManaCheat(RuntimeLocalBattlePlayer player)
		{
			Name = "Add Mana";
			Description = "SomeBody give you 2 Mana ";
			this.players = player;
		}

		public async Awaitable Execute(CheatContext context)
		{
			await Awaitable.MainThreadAsync();
			players.BattlePlayer.AddOrRemoveMana(1);
			Debug.Log(players);
			ModifyPlayerManaCommand command = new ModifyPlayerManaCommand(players.BattlePlayer, 1);
			command.Run(players.BattlePlayer.BattlePhase);
			Debug.Log(
				$"[AddHealthCheat] SomeBody give you Mana a bit... now you have : {players.BattlePlayer.CurrentMana} HP ");
		}
	}

}