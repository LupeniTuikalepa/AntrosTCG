using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Entities;
using UnityEngine;

namespace ATCG.Battle.Commands.Listeners
{
    public interface IEntitySignalListener : 
        IBaseSignalListener<EntityCommandSignal>
    {
        Entity Target { get; }
        
        bool ICommandListener<EntityCommandSignal>.Accepts(CommandContext context, EntityCommandSignal command)
        {
            return HasSource(command) && command.Target == Target;
        }
    }
}