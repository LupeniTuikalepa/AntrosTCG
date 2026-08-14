using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.PassiveSystem.Core;
using ATCG.Passives.Datas.Datas.Fire;

namespace ATCG.Battle.PassiveSystem.Passives.Fire
{
    public partial struct FogBank : IPassive<FogBankData>
    {
        public IEnumerable<IPassiveCommandListener> GetListeners(FogBankData data, PassiveContext ctx)
        {
            yield return new PassiveCommandListener<EndTurnCommand>(data, ctx.owner)
            {
                accepts = (context, command) =>
                    !ctx.owner.IsAlly(command.GetPlayer(context)), 
                setupContext = SetupContext
            };
        }

        private void SetupContext(
            PassiveContext passiveContext,
            CommandContext commandContext,
            EndTurnCommand command)
        {
            
        }

        public void Tick(FogBankData data, PassiveContext ctx)
        {
            
        }
    }
}