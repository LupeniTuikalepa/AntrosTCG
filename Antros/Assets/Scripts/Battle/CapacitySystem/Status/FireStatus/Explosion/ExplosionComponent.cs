using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Explosion
{
	public struct ExplosionComponent : IStatusComponent
	{
		public class ExplosionListener :ICommandListener<DamageCommand>
		{
			public EntityAddress Target { get; }
			private StatusData data;
			
			public ExplosionListener(EntityAddress target)
			{
				Target = target;
			}
			void ICommandListener<DamageCommand>.Trigger(CommandContext context, DamageCommand command)
			{
				if(command.Source == Explosion.ExplosionStatus.EXPLOSION_SOURCE)
					return;
				
				var targetEntityAddress = command.TargetEntityAddress(context.World);
				var battlePhase = context.battlePhase;
				
				if (Target.HasStatus<ExplosionStatus>(out var statusTag))
				{
					var jsp = statusTag.GetValue().data;
					data = jsp;
					if (data.TryGet(out IStatusContainer component))
					{
						component.Tick(data,targetEntityAddress, new StatusContext(battlePhase));
					}
				}
			}
		}
		StatusData IStatusComponent.StatusStatusData => ExplosionStatus;
		public ExplosionStatusData ExplosionStatus { get; }
		public ExplosionListener Listener { get; private set; }
		
		public ExplosionComponent(ExplosionStatusData explosionStatus, ExplosionListener listener)
		{
			ExplosionStatus = explosionStatus;
			Listener = listener;
		}
		
		public void Watch(EntityAddress target)
		{
			Listener?.Unregister();
			Listener = new ExplosionListener(target);
			Listener.Register();
		}

		void IEntityComponent.Dispose()
		{
			Listener?.Unregister();
		}
	}
}