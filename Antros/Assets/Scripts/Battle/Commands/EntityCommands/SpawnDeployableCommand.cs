using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Deployables;
using ATCG.Battle.Entities.Runtime.Deployables;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.HexGrids;
using Helteix.Tools;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class SpawnDeployableCommand : PlayerCommand<SpawnDeployableCommand.Infos>
    {
        public struct Infos : ICommandInfos
        {
            public readonly DeployableData data;
            public readonly BattleCellAspect cell;
            public readonly DeployableAspect deployable;
            public readonly BattleGrid grid;

            public HexCoordinates Destination => cell.Coordinate; 
            public Infos(DeployableData data, BattleCellAspect cell, DeployableAspect deployable, BattleGrid grid)
            {
                this.data = data;
                this.cell = cell;
                this.deployable = deployable;
                this.grid = grid;
            }
        }

        private readonly IBattlePlayer player;
        private readonly DeployableData data;
        private readonly BattleCellAspect cell;
        private readonly EntityAddress caster;

        private HexCoordinates Destination => cell.Coordinate;

        public SpawnDeployableCommand(IBattlePlayer player, DeployableData data, BattleCellAspect cell, EntityAddress caster) : base(player)
        {
            this.player = player;
            this.data = data;
            this.cell = cell;
            this.caster = caster;
        }

        protected override void Process(in CommandContext context)
        {
            DeployableAspect deployableAspect = DeployableAspect.CreateAspect(context.World,
                new DeployableAspect.Setup()
                {
                    caster = caster,
                    coordinates = Destination,
                    grid = context.Grid,
                    battleID = BattleID.CreateNew()
                });

            infos = new Infos(data, cell, deployableAspect, context.Grid);

            if (data.TryGet(out IDeployableContainer container))
                container.SetupEntity(data, deployableAspect);
            
        }
    }
}