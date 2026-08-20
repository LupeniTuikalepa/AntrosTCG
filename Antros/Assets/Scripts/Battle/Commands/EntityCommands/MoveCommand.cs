using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MoveCommand : EntityCommand<DeltaInfos<HexCoordinates>>
    {
        public readonly HexCoordinates destination;

        public MoveCommand(EntityAddress address, HexCoordinates destination) : base(address)
        {
            this.destination = destination;
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