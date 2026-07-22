using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Commands.GameCommands
{
    public class SpawnConstructionCommand : PlayerCommand<SpawnConstructionCommand.Infos>
    {
        private readonly ConstructionBattleCard constructionBattleCard;
        private readonly GameObject prefab;
        private readonly HexCoordinates destination;

        public struct Infos : ICommandInfos
        {
            public readonly GameObject prefab;
            public readonly HexCoordinates destination;
            public readonly ConstructionAspect construction;
            public readonly BattleGrid grid;

            public Infos(GameObject prefab, HexCoordinates destination, ConstructionAspect construction, BattleGrid grid)
            {
                this.prefab = prefab;
                this.destination = destination;
                this.construction = construction;
                this.grid = grid;
            }
        }
        
        public SpawnConstructionCommand(IBattlePlayer battlePlayer, ConstructionBattleCard constructionBattleCard, HexCoordinates destination) : base(battlePlayer)
        {
            this.constructionBattleCard = constructionBattleCard;
            prefab = constructionBattleCard.Prefab;
            this.destination = destination;
        }

        protected override void Process(in CommandContext context)
        {
            ConstructionAspect construction = ConstructionAspect.CreateAspect(context.World,
                new ConstructionAspect.Setup()
                {
                    prefab = prefab,
                    card =  constructionBattleCard,
                    coordinates = destination,
                    grid = context.Grid,
                    battleID = constructionBattleCard.ID
                });

            infos = new Infos(prefab, destination, construction, context.Grid);
        }
    }
}