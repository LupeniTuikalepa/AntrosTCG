using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MoveCommand : EntityCommand<MoveCommand.Infos>
    {
        public struct Infos
        {
            public HexCoordinates from;
            public HexCoordinates to;
        }

        public readonly HexCoordinates destination;

        public MoveCommand(Entity sourceEntity, HexCoordinates destination) : base(sourceEntity)
        {
            this.destination = destination;
        }

        //TODO use correct component
        protected override void Process(in CommandContext context)
        {
            if (Target.TryGetComponent<GridMemberComponent>(context.World, out var gridEntityComponentRef))
            {
                ref GridMemberComponent component = ref gridEntityComponentRef.GetValue();
                infos.from = component.coordinates;

                component.coordinates =destination;

                infos.to = component.coordinates;
            }
        }
    }
}