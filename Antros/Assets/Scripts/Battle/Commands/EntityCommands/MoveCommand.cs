using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MoveCommand : EntityCommand
    {
        public readonly HexCoordinates destination;

        public MoveCommand(Entity sourceEntity, HexCoordinates destination) : base(sourceEntity)
        {
            this.destination = destination;
        }

        //TODO use correct component
        protected override void Process(in GameCommandContext context)
        {
            if (TargetEntity.TryGetComponent<GridMemberComponent>(context.World, out var gridEntityComponentRef))
            {
                ref GridMemberComponent component = ref gridEntityComponentRef.GetValue();
                component.coordinates =destination;
            }
        }
    }
}