using System.Linq;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Debugging.Debugging.Battle.ChoicePhase;
using Cheats.Core;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Debugging.Debugging.Battle
{
	public class KillEntityCheat : ICheat
	{
		private readonly LocalBattlePlayer player;
		public string Name { get; }
		public string Description { get; }
		
		public KillEntityCheat(LocalBattlePlayer player)
		{
			this.player = player;
			Name = "Kill Entity";
			Description = "Take the life of whomever you want.";
		}

		public async Awaitable Execute(CheatContext context)
		{
			using (DictionaryPool<string, EntityAddress>.Get(out var bucket))
			{
				CheatUtilities.FillBucket<HealthComponent>(bucket,player);

				CheatsChoicePhase choicePhase = new CheatsChoicePhase( player, bucket.Keys.ToList());
				
				string result = await choicePhase.Run();

				if (bucket.TryGetValue(result, out var address))
				{
					DeathCommand deathCommand = new DeathCommand(address);
					deathCommand.Run(player.BattlePhase);
					bucket.Remove(result);
				}
			}
		}

		
	}
}