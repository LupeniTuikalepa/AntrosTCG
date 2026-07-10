using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Construction;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Construction;
using ATCG.HexGrids;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Commands.GameCommands
{
    public class SpawnConstructionCommand : PlayerCommand<SpawnConstructionCommand.Infos>
    {
        private readonly HeroBattleCard heroBattleCard;
        private readonly ConstructionData data;
        private readonly BattleCellAspect cell;

        public struct Infos : ICommandInfos
        {
            public readonly ConstructionData data;
            public readonly BattleCellAspect cell;
            public readonly ConstructionAspect construction;
            public readonly BattleGrid grid;

            public Infos(ConstructionData data, BattleCellAspect cell, ConstructionAspect construction, BattleGrid grid)
            {
                this.data = data;
                this.cell = cell;
                this.construction = construction;
                this.grid = grid;
            }
        }
        
        private HexCoordinates Destination => cell.Coordinate;
        
        //TODO replace HeroBattleCard with ConstructionCard
        public SpawnConstructionCommand(IBattlePlayer battlePlayer, HeroBattleCard heroBattleCard, ConstructionData data, BattleCellAspect cell) : base(battlePlayer)
        {
            this.heroBattleCard = heroBattleCard;
            this.data = data;
            this.cell = cell;
        }

        protected override void Process(in CommandContext context)
        {
            ConstructionAspect construction = ConstructionAspect.CreateAspect(context.World,
                new ConstructionAspect.Setup()
                {
                    data = data,
                    card =  heroBattleCard,
                    coordinates = Destination,
                    grid = context.Grid,
                    battleID = heroBattleCard.ID
                });

            infos = new Infos(data, cell, construction, context.Grid);

            if (data.TryGet(out IConstructionContainer container))
                container.SetupEntity(data, construction);
        }
    }
}