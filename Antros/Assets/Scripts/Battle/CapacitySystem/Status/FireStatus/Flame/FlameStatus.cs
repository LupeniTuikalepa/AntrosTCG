using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle
{
    public partial struct FlameStatus : IStatus<FlameStatusData>
    {
	    private ChannelKey channelKey;
	    
	    
	    public void Apply(FlameStatusData data, EntityAddress target, StatusContext context)
	    {
		    if (target.HasComponent<FlameStatusComponent>())
		    {
			    if (target.TryGetComponent<StatusDurationController<FlameStatusComponent>>(out var controller))
			    {
				    controller.GetValue().AddOrRemoveTicks(1);
				    
			    }
		    }
		    target.ApplyStatus(new FlameStatusComponent(data, channelKey),
			    new StatusDurationController<FlameStatusComponent>(),
			    context);
	    }

	    public void Remove(FlameStatusData data, EntityAddress address, StatusContext context)
	    {
		    address.RemoveStatus<FlameStatusComponent>(address, context);
	    }

	    public void Tick(FlameStatusData data, EntityAddress address, StatusContext context)
	    {
		    StatusManager.Trigger<FlameStatusComponent>(address, context);
		    
		    int damage = data.Damage;
		    if (address.TryGetComponentRO<StatusDurationController<FlameStatusComponent>>(out var controller))
		    {
			    damage *= controller.RemainingTicks;
		    }

		    if (address.HasComponent<HealthComponent>())
		    {
			    var damageCommand = new DamageCommand(damage, address);
			    damageCommand.Run(context.battlePhase);
		    }
	    }

	    public void TickAll(FlameStatusData data, StatusContext context)
	    {
	    }
    }
}
