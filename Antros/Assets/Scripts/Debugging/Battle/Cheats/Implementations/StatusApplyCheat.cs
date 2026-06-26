using System.Linq;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Capacities.Data.Status;
using ATCG.Debugging.Debugging.Battle;
using ATCG.Debugging.Debugging.Battle.ChoicePhase;
using Cheats.Core;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Debugging
{
    public class StatusApplyCheat :ICheat
    {
	    public string Name { get; }
	    public string Description { get; }
	    
	    private readonly LocalBattlePlayer player;
	    
	    public StatusApplyCheat(LocalBattlePlayer player)
	    {
		    Name = " Apply Status ";
		    Description = " Infect whomever you want. ";
		    this.player = player;
	    }

	    public async Awaitable Execute(CheatContext context)
	    {
		    using (DictionaryPool<string, EntityAddress>.Get(out var bucket))
		    {
			    CheatUtilities.FillBucket<HealthComponent>(bucket,player);
			    
			    CheatsChoicePhase cheatsChoicePhase = new CheatsChoicePhase(player, bucket.Keys.ToList());
			    string result = await cheatsChoicePhase.Run();

			    if (bucket.TryGetValue(result, out EntityAddress entity))
			    {
				   StatusData[] datas = Resources.LoadAll<StatusData>("Database/Status");
				   foreach (StatusData data in datas)
				   {
					   StatusApplyCommand  command = new StatusApplyCommand(entity, data);
					   command.Run(player.BattlePhase);
					   Debug.Log($"{entity.entity.id} take a {command}");
				   }
			    }
		    }
	    }
	    
    }
}
