using System.Linq;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Capacities.Data.Status;
using ATCG.Debugging.Debugging.Battle.ChoicePhase;
using Cheats.Core;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Debugging.Debugging.Battle.Cheats.Implementations
{
	public class StatusAllCheat :ICheat
	{
		public string Name { get; }
		public string Description { get; }
	    
		private readonly LocalBattlePlayer player;
	    
		public StatusAllCheat(LocalBattlePlayer player)
		{
			Name = " Apply All Status ";
			Description = " Tick all the infected. ";
			this.player = player;
		}

		public async Awaitable Execute(CheatContext context)
		{
			StatusData[] datas = Resources.LoadAll<StatusData>("Database/Status");
			foreach (StatusData data in datas)
			{
				StatusTickAllCommand command = new StatusTickAllCommand(data);
				await command.RunAsync(player.BattlePhase);
			} 
		}
	}

}