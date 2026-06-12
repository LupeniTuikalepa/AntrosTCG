using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.HexGrids.Utility;
using ATCG.Metrics;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.EntityCommands
{
	public class PhysicalAttackCommand : EntityCommand
	{
		private readonly int strength;

		public PhysicalAttackCommand(EntityAddress targetEntity, int strength) : base(targetEntity)
		{
			this.strength = strength;
		}
		
		private void GetHitMember(in GameCommandContext context, List<EntityAddress> addresses)
		{
			EntityAddress address = TargetEntityAddress(context.World);
			BattlePhase battlePhase = context.battlePhase;
			
			if (address.TryGetComponentRO(out BattleGridElementComponent battleGridElement))
			{
				var coordinate = battleGridElement.coordinates;
				Debug.Log(coordinate);
		        
				var hitRing = coordinate.GetRing(GameMetrics.Current.BasicAttackRange);
				

				foreach (var hit in hitRing)
				{
					if (hit == coordinate)
						continue;
					
					if (!battlePhase.BattleGrid.TryGetBattleCell(hit, out BattleCellAspect cell))
						continue;
			        
					foreach (ComponentRef<BattleGridElementComponent> componentRef in cell.GetMembers())
					{
						addresses.Add(componentRef.Address);
					}
				}
			}
		}

		protected override void Process(in GameCommandContext context)
		{
			new ModifyPlayerManaCommand(TargetEntity, GameMetrics.Current.BasicAttackCost);
			
			using (ListPool<EntityAddress>.Get(out var addresses))
			{
				GetHitMember(in context, addresses);
				
				foreach (var hit in addresses)
				{
					DamageCommand command = new DamageCommand(strength, hit.entity);
					Embed(in context, command);
					Debug.Log(hit.entity.id);
				}
			}
		}
	}
}