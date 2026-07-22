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
        private readonly ConstructionBattleCard constructionBattleCard;
        private readonly ConstructionData constructionData;
        private readonly HexCoordinates destination;

        public struct Infos : ICommandInfos
        {
            public readonly ConstructionData constructionData;
            public readonly HexCoordinates destination;
            public readonly ConstructionAspect construction;
            public readonly BattleGrid grid;

            public Infos(ConstructionData constructionData, HexCoordinates destination, ConstructionAspect construction, BattleGrid grid)
            {
                this.constructionData = constructionData;
                this.destination = destination;
                this.construction = construction;
                this.grid = grid;
            }
        }
        
        public SpawnConstructionCommand(IBattlePlayer battlePlayer, ConstructionBattleCard constructionBattleCard, HexCoordinates destination) : base(battlePlayer)
        {
            this.constructionBattleCard = constructionBattleCard;
            constructionData = constructionBattleCard.ConstructionData;
            this.destination = destination;
        }

        protected override void Process(in CommandContext context)
        {
            ConstructionAspect construction = ConstructionAspect.CreateAspect(context.World,
                new ConstructionAspect.Setup()
                {
                    constructionData = constructionData,
                    card =  constructionBattleCard,
                    coordinates = destination,
                    grid = context.Grid,
                    battleID = constructionBattleCard.ID
                });

            infos = new Infos(constructionData, destination, construction, context.Grid);

            if (constructionData.TryGet(out IConstructionContainer container))
                container.SetupEntity(constructionData, construction);
        }
    }
}