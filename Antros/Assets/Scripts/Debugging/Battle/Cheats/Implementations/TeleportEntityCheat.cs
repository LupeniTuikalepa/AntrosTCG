using System.Collections.Generic;
using System.Linq;
using ATCG.Battle;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Debugging.Debugging.Battle.ChoicePhase;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
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
				CheatUtilities.FillBucket<MovementComponent>(bucket,player);
				
				CheatsChoicePhase choicePhase = new CheatsChoicePhase(player, bucket.Keys.ToList());
				string result = await choicePhase.Run();

				if (bucket.TryGetValue(result, out EntityAddress address))
				{
					var battlePatternController = new BattlePatternController(player.BattlePhase.BattleGrid);
					
					using HexPatternBuilder allCell = new HexPatternBuilder(HexCoordinates.Zero,
						battlePatternController).With(new EverythingPattern());

					var filter = new AspectFilter<BattleCellAspect>();
					
					var phase = new SelectEntityPhase<AspectFilter<BattleCellAspect>>(player,filter,allCell);
					EntityAddress[] resultCoordinate = await phase.Run();
					if (resultCoordinate.Length > 0)
					{
						EntityAddress first = resultCoordinate[0];
						if (first.TryGetComponentRO(out GridMemberComponent gridMember))
						{
							MoveCommand moveCommand = new MoveCommand(address, gridMember.coordinates);
					
							if(address.TryGetComponentRO(out GridMemberComponent component))
								Debug.Log($"{address.entity.id} : is teleported to {component.coordinates}");
					
							moveCommand.Run(player.BattlePhase);
						}
					}
					
				}
			}
		}
		
	}
}