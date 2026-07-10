using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle
{
    public readonly struct FlameStatusComponent : IStatusComponent
    {
	    StatusData IStatusComponent.StatusData => data;
	    private readonly FlameStatusData data;

	    public FlameStatusComponent(FlameStatusData data)
	    {
		    this.data = data;
	    }
    }
}