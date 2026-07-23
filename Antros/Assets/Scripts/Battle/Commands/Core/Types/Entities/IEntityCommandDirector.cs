using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Entities
{
    public interface IEntityCommandDirector<in T> : ICommandDirector<T> where T : IEntityCommand
    {
        Entity Entity { get; }
        bool ICommandDirector<T>.CanPlay(T command)
        {
            return command.Target == Entity;
        }
    }
}