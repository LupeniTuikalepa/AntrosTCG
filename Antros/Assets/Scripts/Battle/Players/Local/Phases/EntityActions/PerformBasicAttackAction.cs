using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime.Heroes;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns;
using ATCG.Battle.Grids.Patterns.Building;
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
	    private readonly struct EnemyFilter : IEntityFilter
	    {
		    private readonly IBattlePlayer player;

		    private readonly HexPatternBuilder patterns;
		    public EnemyFilter(IBattlePlayer player, HexPatternBuilder patterns)
		    {
			    this.player = player;
			    this.patterns = patterns;
		    }
		    public bool Accepts(EntityAddress address)
		    {
			    if(!address.HasComponent<HealthComponent>())
				    return false;

			    if(address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer) && belongsToPlayer.IsAllieOf(player))
				    return false;

			    if(!address.TryGetComponentRO(out HexCoordinatesComponent battleGridElement))
				    return false;

			    return patterns.Contains(battleGridElement.coordinates);
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
	        if (!address.TryGetComponentRO(out HexCoordinatesComponent battleGridElement))
		        return;

	        HexCoordinates center = battleGridElement.coordinates;
	        int radius = GameMetrics.Current.BasicAttackRange;

	        using HexPatternBuilder builder = new HexPatternBuilder(center)
		        .With(new SpiralPattern(radius))
		        .Without(center);

	        var filter = new EnemyFilter(player, builder);
	        EntityAddress[] result = await new SelectEntityPhase<EnemyFilter>(filter);

	        for (int i = 0; i < result.Length; i++)
	        {
		        EntityAddress target = result[i];
		        var command = new BasicAttackCommand(address, target, strength);
		        await command.Run(battlePhase);
	        }
        }
    }
}