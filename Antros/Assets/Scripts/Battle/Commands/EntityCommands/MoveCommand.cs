using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class MoveCommand : EntityCommand
    {
        public readonly HexCoordinates destination;

        public MoveCommand(Entity targetEntity, HexCoordinates destination) : base(targetEntity)
        {
            this.destination = destination;
        }

        //TODO use correct component
        protected override void Process(in GameCommandContext context)
        {
            if (TargetEntity.TryGetComponent<BattleGridElementComponent>(context.World, out var gridEntityComponentRef))
            {
                ref BattleGridElementComponent component = ref gridEntityComponentRef.GetValue();
                component.coordinates =destination;
            }
        }
    }
}