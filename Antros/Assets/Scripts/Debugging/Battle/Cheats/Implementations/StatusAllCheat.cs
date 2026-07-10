using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Players.Local;
using ATCG.Capacities.Data.Status;
using Cheats.Core;
using UnityEngine;

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