using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime.Heroes;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Phases;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Utility;
using ATCG.Metrics;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class PerformBasicAttackAction : IEntityAction
    {
	    private struct  EnemieFilter : IEntityFilter
	    {
		    private readonly IBattlePlayer player;

		    private readonly HashSet<HexCoordinates> targets;
		    public EnemieFilter(IBattlePlayer player, HashSet<HexCoordinates> targets)
		    {
			    this.player = player;
			    this.targets = targets;
		    }
		    public bool Accepts(EntityAddress address)
		    {
			    if(!address.HasComponent<HealthComponent>())
				    return false;
			    if(address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer) && belongsToPlayer.IsAllieOf(player))
				    return false;
			    if(!address.TryGetComponentRO(out BattleGridElementComponent battleGridElement))
				    return false;
			    return targets.Contains(battleGridElement.coordinates);
		    }
	    }
        private readonly int strength;
        private readonly IBattlePlayer player;
        public int ManaCost => GameMetrics.Current.BasicAttackCost;
        public PerformBasicAttackAction(int strength,IBattlePlayer player)
        {
	        this.strength = strength;
	        this.player = player;
        }
        
        public async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
	        using (HashSetPool<HexCoordinates>.Get(out var coordinates))
	        {
		        if(!address.TryGetComponentRO(out BattleGridElementComponent battleGridElement))
			        return;
		        
		        int radius = GameMetrics.Current.BasicAttackRange;
		        HexCoordinates center = battleGridElement.coordinates;
		        
		        foreach (var coordinate in center.GetSpiral(radius))
		        {
			        if(coordinate != center)
				        coordinates.Add(coordinate);
		        }
		        
		        EnemieFilter filter = new EnemieFilter(player, coordinates);

		        SelectEntityPhase<EnemieFilter> phase = new SelectEntityPhase<EnemieFilter>(filter);

		        EntityAddress[] result = await phase;

		        for (int i = 0; i < result.Length; i++)
		        {
			        EntityAddress target = result[i];
			        
			        if (target.TryGetComponentRO(out BattleGridElementComponent component))
			        {
				        var command = new PhysicalAttackCommand(address, strength);
				        await command.Run(battlePhase);
			        }
		        }
	        }

        }
        
    }
}