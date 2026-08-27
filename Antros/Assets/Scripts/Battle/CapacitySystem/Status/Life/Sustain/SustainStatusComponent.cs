using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Data.Status.Life;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Life.Sustain
{
	public struct SustainStatusComponent : IStatusComponent
	{
		public class SustainListener : ICommandListener<DamageCommand>
		{
			public EntityAddress target;
			private StatusData data;

			public SustainListener(EntityAddress target)
			{
				this.target = target;
			}
			
			void ICommandListener<DamageCommand>.Trigger(CommandContext context, DamageCommand command)
			{
				var attacker = command.attacker;
				var battlePhase = context.battlePhase;
				
				if (!attacker.IsValid || attacker != target)
					return;

				int finalDamage = command.FinalDamageInfo;
				if (finalDamage <= 0)
					return;

				if (target.HasStatus<SustainStatus>(out var statusTag))
				{
					HealCommand healCommand = new HealCommand(finalDamage, target);
					healCommand.Run(battlePhase);
				}
			}
		}
		
		StatusData IStatusComponent.StatusStatusData => SustainStatus;
		public SustainStatusData SustainStatus { get; }
		public SustainListener Listener { get; private set; }
		
		public SustainStatusComponent (SustainStatusData sustainStatus, SustainListener listener)
		{
			SustainStatus = sustainStatus;
			Listener = listener;
		}
		
		public void Watch(EntityAddress target)
		{
			Listener?.Unregister();
			Listener = new SustainListener(target);
			Listener.Register();
		}

		void IEntityComponent.Dispose()
		{
			Listener?.Unregister();
		}
	}
}