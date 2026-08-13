using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Status.Berserk;
using ATCG.Battle.Cards;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Cutscenes;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Cutscenes;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
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

	        BattlePatternController patternController = new BattlePatternController(BattleGrid);
	        using HexPatternBuilder builder = new HexPatternBuilder(center, patternController)
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

	        using (CommandManager.BeginGroup($"[{address.entity.id}] Entity Basic Attack"))
	        {
		        //Le player a l'origine de l'action perd de la mana
		        ModifyPlayerManaCommand manaCost =
			        new ModifyPlayerManaCommand(fromPlayer, -GameMetrics.Current.BasicAttackCost);
		        await manaCost.RunAsync(battlePhase);

		        // If the attacker is a hero with an attack cutscene, play it and land the damage on
		        // the "Hit" marker. Otherwise keep the instant-damage behaviour (no cutscene).
		        AttackCutscene attackCutscene =
			        address.TryGetComponentRO(out BattleCardComponent cardComponent)
			        && cardComponent.battleCard is HeroBattleCard heroCard
				        ? heroCard.Data.AttackCutscene
				        : null;

		        EntityAddress[] targets = result;

		        if (attackCutscene != null)
		        {
			        // QTE effectiveness [0,1] scales the hit; a cutscene with no QTE clips reads back
			        // 1 (full damage), so non-QTE attacks are unchanged. Tune the mapping as needed.
			        QteResultAccumulator qteResults = new();
			        await BattleCutscenes.Play(attackCutscene, battlePhase, address,
				        new Dictionary<string, Action>
				        {
					        [AttackCutscene.HIT] = () =>
					        {
						        int scaled = Mathf.Max(0, Mathf.RoundToInt(strength * qteResults.Read()));
						        for (int i = 0; i < targets.Length; i++)
							        new BasicAttackCommand(address, targets[i], scaled).Run(battlePhase);
					        }
				        }, qteResults);
		        }
		        else
		        {
			        for (int i = 0; i < targets.Length; i++)
				        await new BasicAttackCommand(address, targets[i], strength).RunAsync(battlePhase);
		        }
	        }
        }
    }
}