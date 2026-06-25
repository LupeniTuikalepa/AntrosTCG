using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Players.Local.Runtime;
using Cheats.Core;
using UnityEngine;

namespace ATCG.Debugging.Debugging.Battle
{
	public class RemoveMana: ICheat
	{
		public string Name { get; }
		public string Description { get; }
		public readonly RuntimeLocalBattlePlayer players;

		public RemoveMana(RuntimeLocalBattlePlayer player)
		{
			Name = nameof(RemoveMana);
			Description = "SomeBody remove your Mana a bit";
			this.players = player;
		}

		public void Execute(in CheatContext context)
		{
			players.BattlePlayer.AddOrRemoveMana(-1);
			Debug.Log(players);
			ModifyPlayerManaCommand command = new ModifyPlayerManaCommand(players.BattlePlayer, -1);
			command.Run(players.BattlePlayer.BattlePhase);
			Debug.Log(
				$"[AddHealthCheat] SomeBody remove your Mana a bit... now you have : {players.BattlePlayer.CurrentMana} HP ");
		}
	} 
}