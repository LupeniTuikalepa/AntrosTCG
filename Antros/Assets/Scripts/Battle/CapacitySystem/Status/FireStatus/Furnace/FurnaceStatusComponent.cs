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
	        public readonly FurnaceData data;
	        
	        public FurnaceListener(HexCoordinates destination, FurnaceData data)
	        {
		        this.destination = destination;
		        this.data = data;
	        }

	        public bool Accepts(CommandContext context, MoveCommand command)
	        {
		        return command.destination == destination;
	        }

	        public void Trigger(CommandContext context, MoveCommand command)
	        {
		        if(data.TryGet(out IStatusContainer statusContainer))
		        {
			        StatusContext statusContext = new StatusContext(context.battlePhase);
			        statusContainer.Tick(data, command.TargetEntityAddress(context.World), statusContext);
		        }
	        }
        }

        StatusData IStatusComponent.StatusData => FurnaceData;
        
        public FurnaceData FurnaceData { get; }
        public FurnaceListener Listener { get; private set; }

        public FurnaceStatusComponent(FurnaceData data)
        {
	        FurnaceData = data;
            Listener = null;
        }

        public void Watch(EntityAddress target)
        {
	        if (target.TryGetComponentRO<GridMemberComponent>(out var gridMember))
	        {
		        Listener?.UnregisterWatcher();
		        Listener = new FurnaceListener(gridMember.coordinates, FurnaceData);
		        Listener.RegisterWatcher();
	        }
        }

        void IEntityComponent.Dispose() => Listener?.UnregisterWatcher();
    }
}