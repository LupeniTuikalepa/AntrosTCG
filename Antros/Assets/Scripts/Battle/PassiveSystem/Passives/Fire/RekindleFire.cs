using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.PassiveSystem.Core;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Passives.Datas.Datas.Fire;

namespace ATCG.Battle.PassiveSystem.Passives.Fire
{
    public partial struct RekindleFire : IPassive<RekindleFireData>
    {
        public const string TARGET_BATTLE_CELLS = "target_battle_cells";
        
        
        public IEnumerable<IPassiveCommandListener> GetListeners(RekindleFireData data, PassiveContext ctx)
        {
            yield return new PassiveCommandListener<EndTurnCommand>(data, ctx.owner)
            {
                accepts = (context, command) =>
                    !ctx.owner.IsAlly(command.GetPlayer(context)), 
                setupContext = SetupContext
            };
        }

        private void SetupContext(PassiveContext context, CommandContext commandContext, EndTurnCommand command)
        {
            if(!context.owner.TryGetComponentRO<GridMemberComponent>(out var ownerGridMember))
               return;
            
            if(!context.owner.Is<ConstructionAspect>(out var constructionAspect))
                return;
            
            var ownerOrigin = ownerGridMember.coordinates;
            var builder = new HexPatternBuilder(
                ownerOrigin,
                new BattleIgnoreOriginPatternController(commandContext.Grid, ownerOrigin))
                .With(new SpreadPattern(constructionAspect.PassiveRange));

            IEnumerable<BattleCellAspect> battleCells = builder.GetBattleCells(commandContext.Grid);
            
            context.AddProperty(TARGET_BATTLE_CELLS, battleCells);
        }

        public void Tick(RekindleFireData data, PassiveContext ctx)
        {
            if(!ctx.TryGet(TARGET_BATTLE_CELLS, out IEnumerable<BattleCellAspect> battleCells))
                return;

            foreach (var cell in battleCells)       
            {
                foreach (var member in cell.GetMembers())
                {
                    var applyStatusCommand =
                        new ApplyStatusCommand(member.EntityAddress, data.Status, data.AdditionalStack);
                    applyStatusCommand.Run(ctx.battlePhase);
                }
            }
        }
    }
}