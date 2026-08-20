using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Entities;
using UnityEngine;

namespace ATCG.Battle.Commands.Listeners
{
    public interface IEntitySignalDirector : 
        IBaseSignalDirector<EntityCommandSignal>
    {
        Entity Target { get; }

        bool ICommandDirector<EntityCommandSignal>.CanPlay(EntityCommandSignal command)
        {
            return HasSource(command) && command.Target == Target;
        }
    }
}