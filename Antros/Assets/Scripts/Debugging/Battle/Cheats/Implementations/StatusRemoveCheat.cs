using System.Linq;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
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
	public class StatusRemoveCheat : ICheat
	{
		public string Name { get; }
		public string Description { get; }

		private readonly LocalBattlePlayer player;

		public StatusRemoveCheat(LocalBattlePlayer player)
		{
			Name = " Remove Status ";
			Description = " Bless the infected ";
			this.player = player;
		}

		public async Awaitable Execute(CheatContext context)
		{
			using (DictionaryPool<string, EntityAddress>.Get(out var bucket))
			{
				CheatUtilities.FillBucket<HealthComponent>(bucket, player);

				CheatsChoicePhase cheatsChoicePhase = new CheatsChoicePhase(player, bucket.Keys.ToList());
				string result = await cheatsChoicePhase.Run();

				if (bucket.TryGetValue(result, out EntityAddress entity))
				{
					StatusData[] datas = Resources.LoadAll<StatusData>("Database/Status");
					foreach (StatusData data in datas)
					{
						StatusRemoveCommand command = new StatusRemoveCommand(entity, data);
						command.Run(player.BattlePhase);
						Debug.Log($"{entity.entity.id} take a {command}");
					}
				}
			}
		}
	}
}