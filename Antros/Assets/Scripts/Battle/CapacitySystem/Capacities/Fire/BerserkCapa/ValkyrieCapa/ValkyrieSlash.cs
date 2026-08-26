using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops;
using ATCG.Battle.CapacitySystem.Core.Directors;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Berserk;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.Capacities;
using ATCG.Cutscenes;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct ValkyrieSlash : ICapacity<ValkyrieSlashData>
	{
		private bool hasBerserk;
		private int count;
		public void GetHitPattern(ValkyrieSlashData data, ref HexPatternBuilder builder, BattleGrid battleGrid,
			HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			if (!hasBerserk)
				builder = builder.With(new SpiralPattern(data.Range)).Without(casterOrigin);
			else
				builder = builder.With(new SpiralPattern(data.BerserkRange)).Without(casterOrigin);
		}

		public void GetTargets(ValkyrieSlashData data, BattleCellAspect battleCell, CapacityTargets output,
			IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				EntityAddress address = member.EntityAddress;
				if(address.HasComponent<HealthComponent>()&& !address.IsAlly(castingPlayer))
					output.Add(address, CapacityTags.MEMBER);
			}
		}
		private partial void ExecuteInjectBerserk(ValkyrieSlashData data, CapacityStepContext ctx)
        {
	        if (ctx.capacityPhase.TryGetProperty("InjectBerserk", out bool alreadyInjected) && alreadyInjected)
		        return;
	        ctx.capacityPhase.InjectProperty("InjectBerserk", true);

	        Debug.Log("Une pas deux");
	        
            int enemyCount = 0;
            foreach (var cell in ctx.patternBuilder.GetBattleCells(ctx.BattleGrid))
            {
                foreach (var member in cell.GetMembers())
                {
                    EntityAddress occupant = member.EntityAddress;
                    if (occupant.IsValid && occupant != ctx.Caster && occupant.HasComponent<HealthComponent>() && !ctx.IsAlly(occupant))
                    {
                        enemyCount++;
                    }
                }
            }
            ctx.capacityPhase.InjectProperty(ValkyrieSlashData.ENNEMIES_COUNT, enemyCount);
            if (enemyCount >= data.EnnemyQuantitiesApplyStatus)
            {
                var status = new ApplyStatusCommand(ctx.Caster, data.status);
                status.Run(ctx.BattlePhase);
                hasBerserk = true;
            }
            else
            {
                hasBerserk = false;
            }
        }

        private partial void ExecuteValkyrieSlash(ValkyrieSlashData data, CapacityStepContext ctx)
        {
            using (ListPool<EntityAddress>.Get(out var enemies))
            {
                foreach (var cell in ctx.patternBuilder.GetBattleCells(ctx.BattleGrid))
                {
                    foreach (var member in cell.GetMembers())
                    {
                        EntityAddress occupant = member.EntityAddress;
                        if (occupant.IsValid && occupant != ctx.Caster && occupant.HasComponent<HealthComponent>() && ctx.IsAlly(occupant))
                        {
                            enemies.Add(occupant);
                        }
                    }
                }
                int currentIndex = ctx.loop - 1;
                
                if (currentIndex >= 0 && currentIndex < enemies.Count)
                {
                    EntityAddress targetEnemy = enemies[currentIndex];
                    
                    if (!ctx.Caster.HasStatus<BerserkStatus>())
                    {
                        var damage = new DamageCommand(data.Damage, targetEnemy);
                        damage.Run(ctx.BattlePhase);
                    }
                    else
                    {
                        var damage = new DamageCommand(data.BerserkRange, targetEnemy);
                        damage.Run(ctx.BattlePhase);
                    }
                }
            }
        }

        private partial void ExecuteNeedRemoveBerserkOrNot(ValkyrieSlashData data, CapacityStepContext ctx)
        {
	        if (ctx.capacityPhase.TryGetProperty("NeedRemoveBerserkOrNot", out bool alreadyInjected) && alreadyInjected)
		        return;
	        ctx.capacityPhase.InjectProperty("NeedRemoveBerserkOrNot", true);
	        Debug.Log("Un seul Log pas plus");
			
            if (ctx.Caster.HasStatus<BerserkStatus>())
            {
                var status = new RemoveStatusCommand(ctx.Caster, data.status);
                status.Run(ctx.BattlePhase);
                hasBerserk = false;
            }
            else
            {
	            var status = new ApplyStatusCommand(ctx.Caster, data.status);
	            status.Run(ctx.BattlePhase);
            }
        }
		
	}
}