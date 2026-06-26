using System.Collections.Generic;
using System.Linq;
using ATCG.Battle;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Debugging.Debugging.Battle.ChoicePhase;
using ATCG.HexGrids;
using Cheats.Core;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Debugging.Debugging.Battle
{
	public class TeleportEntityCheat : ICheat
	{
		public string Name { get; }
		public string Description { get; }
		private readonly LocalBattlePlayer player;

		public TeleportEntityCheat(LocalBattlePlayer player)
		{
			Name = "Teleport";
			Description = "Teleport to an entity";
			this.player = player;
		}

		public async Awaitable Execute(CheatContext context)
		{
			using (DictionaryPool<string , EntityAddress>.Get(out var bucket))
			{
				CheatUtilities.FillBucket<GridMemberComponent>(bucket,player);
				
				CheatsChoicePhase choicePhase = new CheatsChoicePhase(player, bucket.Keys.ToList());
				string result = await choicePhase.Run();

				if (bucket.TryGetValue(result, out EntityAddress address))
				{
					HexCoordinates coordinates = new HexCoordinates(0,0);
					MoveCommand moveCommand = new MoveCommand(address,coordinates);
					
					if(address.TryGetComponentRO(out GridMemberComponent component))
					Debug.Log($"{address.entity.id} : is teleported to {component.coordinates}");
					
					moveCommand.Run(player.BattlePhase);
				}
			}
		}
		
	}
}