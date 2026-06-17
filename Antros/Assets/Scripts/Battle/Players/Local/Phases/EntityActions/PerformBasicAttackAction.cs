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

		    public EnemyFilter(IBattlePlayer player)
		    {
			    this.player = player;
		    }
		    public bool Accepts(EntityAddress address)
		    {
			    if(!address.HasComponent<HealthComponent>())
				    return false;

			    if(address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer) && belongsToPlayer.IsAllieOf(player))
				    return false;

			    return true;
		    }
	    }

	    private readonly int strength;


        public override int ManaCost => GameMetrics.Current.BasicAttackCost;

        public PerformBasicAttackAction(LocalBattlePlayer fromPlayer, int strength) : base(fromPlayer)
        {
	        this.strength = strength;
        }

        public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
	        if (!address.TryGetComponentRO(out GridMemberComponent battleGridElement))
		        return;

	        HexCoordinates center = battleGridElement.coordinates;
	        int radius = GameMetrics.Current.BasicAttackRange;

	        using HexPatternBuilder builder = new HexPatternBuilder(center)
		        .With(new SpiralPattern(radius))
		        .Without(center);

	        //Si l'entité qui attaque appartient a un joueur, on l'utilise. Sinon, on utilise le joueur qui a lancé l'action d'attaque.
	        IBattlePlayer entityPlayer = address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent) ?
		        belongsToPlayerComponent.GetPlayer(battlePhase) :
		        fromPlayer;

	        var filter = new EnemyFilter(entityPlayer);
	        EntityAddress[] result = await new SelectEntityPhase<EnemyFilter>(fromPlayer, filter, builder);
	        if(result.Length == 0)
		        return;

	        //Le player a l'origine de l'action perd de la mana
	        ModifyPlayerManaCommand manaCost = new ModifyPlayerManaCommand(fromPlayer, GameMetrics.Current.BasicAttackCost);
	        await manaCost.RunAsync(battlePhase);

	        for (int i = 0; i < result.Length; i++)
	        {
		        EntityAddress target = result[i];
		        BasicAttackCommand command = new BasicAttackCommand(address, target, strength);
		        await command.RunAsync(battlePhase);
	        }
        }
    }
}