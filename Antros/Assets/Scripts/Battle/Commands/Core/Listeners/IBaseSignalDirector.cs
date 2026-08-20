using System;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Players;
using ATCG.Databases;

namespace ATCG.Battle.Commands.Listeners
{
    public interface IBaseSignalDirector<in T> : ICommandDirector<T> where T : ICommandSignal
    {
        GameDatabaseObject[] Sources { get; }

        bool HasSource(T command)
        {
            for (int i = 0; i < Sources.Length; i++)
            {
                var source = Sources[i];
                if(source.ID == command.Channel)
                    return true;
            }
            
            return false;
        }
        
        bool ICommandDirector<T>.CanPlay(T command)
        {
            return HasSource(command);
        }
    }
}