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

namespace ATCG.Battle.CapacitySystem.Status.Explosion
{
	public struct ExplosionComponent : IStatusComponent
	{
		public class ExplosionListener :ICommandListener<DamageCommand>
		{
			public Entity Target { get; }
			public StatusData data;
			
			public ExplosionListener(Entity target)
			{
				Target = target;
			}
			void ICommandListener<DamageCommand>.Trigger(CommandContext context, DamageCommand command)
			{
				var targetEntityAddress = command.TargetEntityAddress(context.World);
				var battlePhase = context.battlePhase;
				
				if (data.TryGet(out IStatusContainer statusContainer))
				{
					statusContainer.Tick(data,targetEntityAddress,new StatusContext(battlePhase));
				}
			}
		}
		StatusData IStatusComponent.StatusData => Explosion;
		public ExplosionData Explosion { get; }
		public ExplosionListener Listener { get; private set; }
		
		public ExplosionComponent(ExplosionData explosion, ExplosionListener listener)
		{
			Explosion = explosion;
			Listener = listener;
		}
		
		public void Watch(EntityAddress target)
		{
			Listener?.UnregisterWatcher();
			Listener = new ExplosionListener(target);
			Listener.RegisterWatcher();
		}

		void IEntityComponent.Dispose()
		{
			Listener?.UnregisterWatcher();
		}
	}
}