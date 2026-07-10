using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Entities
{
    public interface IEntityCommandListener<in T> : ICommandListener<T> where T : IEntityCommand
    {
        Entity Entity { get; }
        bool ICommandListener<T>.CanPlay(T command)
        {
            return command.Target == Entity;
        }
    }
}