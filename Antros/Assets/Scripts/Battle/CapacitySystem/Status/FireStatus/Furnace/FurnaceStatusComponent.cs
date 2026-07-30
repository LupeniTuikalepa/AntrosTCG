using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;
using ATCG.HexGrids;
using Helteix.ChanneledProperties;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Status.Furnace
{
    public struct FurnaceStatusComponent : IStatusComponent
    {
        public class FurnaceListener : ICommandListener<MoveCommand>
        {
	        public readonly HexCoordinates destination;
	        public readonly FurnaceStatusData statusData;
	        
	        public FurnaceListener(HexCoordinates destination, FurnaceStatusData statusData)
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

        StatusData IStatusComponent.StatusStatusData => FurnaceStatusData;
        
        public FurnaceStatusData FurnaceStatusData { get; }
        public FurnaceListener Listener { get; private set; }

        public FurnaceStatusComponent(FurnaceStatusData statusData)
        {
	        FurnaceStatusData = statusData;
            Listener = null;
        }

        public void Watch(EntityAddress target)
        {
	        if (target.TryGetComponentRO<GridMemberComponent>(out var gridMember))
	        {
		        Listener?.Unregister();
		        Listener = new FurnaceListener(gridMember.coordinates, FurnaceStatusData);
		        Listener.Register();
	        }
        }

        void IEntityComponent.Dispose() => Listener?.Unregister();
    }
}