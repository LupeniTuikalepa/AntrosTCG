using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids.Patterns;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.HexGrids;
using ATCG.Metrics;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle
{
    public class PerformBasicAttackAction : EntityAction
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

			    if(!address.TryGetComponentRO(out GridMemberComponent battleGridElement))
				    return false;

			    return patterns.Contains(battleGridElement.coordinates);
		    }
	    }

	    private readonly int strength;


        public override int ManaCost => GameMetrics.Current.BasicAttackCost;

        public PerformBasicAttackAction(LocalBattlePlayer playerOrigin, int strength) : base(playerOrigin)
        {
	        this.strength = strength;
        }

        public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
	        Debug.Log("Bon");
	        if (!address.TryGetComponentRO(out GridMemberComponent battleGridElement))
		        return;

	        HexCoordinates center = battleGridElement.coordinates;
	        int radius = GameMetrics.Current.BasicAttackRange;

	        using HexPatternBuilder builder = new HexPatternBuilder(center)
		        .With(new SpiralPattern(radius))
		        .Without(center);

	        Debug.Log("Huh");
	        //Si l'entité qui attaque appartient a un jour, on l'utilise. Sinon, on utilise le joueur qui a lancé l'action d'attaque.
	        IBattlePlayer entityPlayer = address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent) ?
		        belongsToPlayerComponent.GetPlayer(battlePhase) :
		        playerOrigin;

	        var filter = new EnemyFilter(entityPlayer, builder);
	        EntityAddress[] result = await new SelectEntityPhase<EnemyFilter>(playerOrigin, filter);
	        if(result.Length == 0)
		        return;

	        //Le player a l'origine de l'action perd de la mana
	        ModifyPlayerManaCommand manaCost = new ModifyPlayerManaCommand(playerOrigin.GetPlayerID(), GameMetrics.Current.BasicAttackCost);
	        await manaCost.Run(battlePhase);

	        for (int i = 0; i < result.Length; i++)
	        {
		        EntityAddress target = result[i];
		        BasicAttackCommand command = new BasicAttackCommand(address, target, strength);
		        await command.Run(battlePhase);
	        }
        }
    }
}