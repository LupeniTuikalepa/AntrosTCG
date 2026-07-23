using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;
using ATCG.Passives.Datas;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.PassiveSystem.Core
{
    public interface IPassiveCommandListener : ICommandListener
    {
        
    }
    public class PassiveCommandListener<T> : IPassiveCommandListener, ICommandListener<T> where T : ICommand
    {
        public Func<CommandContext, T, bool> accepts = delegate { return true; };
        
        public Action<PassiveContext, CommandContext ,T> setupContext = delegate { };  
        
        public readonly PassiveData data;
        private readonly EntityAddress target;

        public PassiveCommandListener(PassiveData data, EntityAddress target)
        {
            this.data = data;
            this.target = target;
        }

        public virtual bool Accepts(CommandContext context, T command) => accepts(context, command);

        public void Trigger(CommandContext context, T command)
        {
            var passiveContext = new PassiveContext(target, context.battlePhase, data);
            setupContext(passiveContext, context, command);
            
            var tickPassiveCommand = new TickPassiveCommand(target, passiveContext);
            command.Inject(context, tickPassiveCommand);
        }
    }
}