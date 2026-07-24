using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;
using ATCG.HexGrids;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Status.Frost.EarthQuake
{
	public struct EarthQuakeStatusComponent : IStatusComponent
	{
		public class EarthQuakeListener : ICommandListener<MoveCommand>
		{
			public readonly HexCoordinates destination;
			public readonly EarthQuakeStatusData statusData;
	        
			public EarthQuakeListener(HexCoordinates destination, EarthQuakeStatusData statusData)
			{
				this.destination = destination;
				this.statusData = statusData;
			}

			public bool Accepts(CommandContext context, MoveCommand command)
			{
				return command.destination == destination;
			}

			public void Trigger(CommandContext context, MoveCommand command)
			{
				if(statusData.TryGet(out IStatusContainer statusContainer))
				{
					StatusContext statusContext = new StatusContext(context.battlePhase);
					statusContainer.Tick(statusData, command.TargetEntityAddress(context.World), statusContext);
				}
			}
		}
		StatusData IStatusComponent.StatusData => EarthQuakeStatusData;
		public EarthQuakeStatusData EarthQuakeStatusData { get; }
		public EarthQuakeListener Listener { get; private set; }

		public EarthQuakeStatusComponent(EarthQuakeStatusData statusData)
		{
			EarthQuakeStatusData = statusData;
			Listener = null;
		}
		public void Watch(EntityAddress target)
		{
			if (target.TryGetComponentRO<GridMemberComponent>(out var gridMember))
			{
				Listener?.Unregister();
				Listener = new EarthQuakeListener(gridMember.coordinates, EarthQuakeStatusData);
				Listener.Register();
			}
		}

		void IEntityComponent.Dispose() => Listener?.Unregister();
	}
}