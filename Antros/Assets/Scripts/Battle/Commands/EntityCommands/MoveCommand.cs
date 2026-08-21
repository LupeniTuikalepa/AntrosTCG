using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Enums;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MoveCommand : EntityCommand<DeltaInfos<HexCoordinates>>
    {
        public readonly HexCoordinates destination;
        public readonly AgentMovementType movementType;

        public MoveCommand(EntityAddress address, HexCoordinates destination, AgentMovementType movementType = AgentMovementType.Default) : base(address)
        {
            this.destination = destination;
            this.movementType = movementType;
        }

        protected override void Process(in CommandContext context)
        {
            if (Target.TryGetComponent<GridMemberComponent>(context.World, out var gridEntityComponentRef))
            {
                ref GridMemberComponent component = ref gridEntityComponentRef.GetValue();
                infos.from = component.coordinates;

                component.coordinates = destination;

                infos.to = component.coordinates;
            }
        }
    }
}