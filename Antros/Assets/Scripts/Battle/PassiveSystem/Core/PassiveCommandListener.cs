using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;
using ATCG.Passives.Datas;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.PassiveSystem.Core
{
    public interface IPassiveCommandListener : ICommandListener
    {
        
    }
    public class PassiveCommandListener<T> : IPassiveCommandListener, 
        ICommandListener<T> where T : ICommand
    {
        public Func<CommandContext, T, bool> accepts = delegate { return true; };
        
        public Action<PassiveContext, CommandContext ,T> setupContext = delegate { };  
        
        public readonly PassiveData data;
        private readonly EntityAddress owner;

        public PassiveCommandListener(PassiveData data, EntityAddress owner)
        {
            this.data = data;
            this.owner = owner;
        }

        public virtual bool Accepts(CommandContext context, T command)
        {
            return accepts(context, command);
        }

        void ICommandListener<T>.Trigger(CommandContext context, T command)
        {
            var passiveContext = new PassiveContext(owner, context.battlePhase, data);
            setupContext(passiveContext, context, command);
            
            var tickPassiveCommand = new TickPassiveCommand(owner, passiveContext);
            command.Inject(context, tickPassiveCommand);
        }
    }
}