using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class PushbackCommand : EntityCommand<DeltaInfos<HexCoordinates>>
    {
        private readonly HexCoordinates destination;

        public PushbackCommand(EntityAddress address, HexCoordinates destination) : base(address)
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