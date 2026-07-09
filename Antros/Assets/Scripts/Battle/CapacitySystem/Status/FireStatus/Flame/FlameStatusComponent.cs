using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle
{
    public readonly struct FlameStatusComponent : IStatusComponent
    {
	    private readonly FlameStatusData data;
	    private readonly ChannelKey channelKey;
        
	    private int Amount => data.Damage;
	    StatusData IStatusComponent.StatusData => data;
        

	    public FlameStatusComponent(FlameStatusData data, ChannelKey channelKey)
	    {
		    this.data = data;
		    this.channelKey = channelKey;
	    }


	    public void Trigger(EntityAddress address, BattlePhase battlePhase)
	    {
		    var damageCommand = new DamageCommand(Amount, address);
		    damageCommand.Run(battlePhase);
	    }
    }
}
