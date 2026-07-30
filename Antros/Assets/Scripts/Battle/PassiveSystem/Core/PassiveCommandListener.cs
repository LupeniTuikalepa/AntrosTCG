using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
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
        private readonly int range;

        public PassiveCommandListener(PassiveData data, EntityAddress owner)
        {
            this.data = data;
            this.owner = owner;

            if (owner.Is<ConstructionAspect>(out var constructionAspect))
                range = constructionAspect.PassiveRange;
            else
                range = -1;
        }

        public virtual bool Accepts(CommandContext context, T command)
        {
            if (range > 0 && !IsInRange(context, command))
                return false;
            
            return accepts(context, command);
        }

        private bool IsInRange(CommandContext context, T command)
        {
            if(command is not IEntityCommand entityCommand)
                return false;

            var address = entityCommand.TargetEntityAddress(context.World);
            if(!address.TryGetComponentRO<GridMemberComponent>(out var targetGridMember))
                return false;
            
            if(!owner.TryGetComponentRO<GridMemberComponent>(out var ownerGridMember))
                return false;
            
            int distance = targetGridMember.coordinates.Distance(ownerGridMember.coordinates);
            return distance <= range;
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