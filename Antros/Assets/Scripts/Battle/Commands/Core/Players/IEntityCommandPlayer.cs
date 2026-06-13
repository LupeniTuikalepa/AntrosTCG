using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Core.Players
{
    public interface IEntityCommandPlayer<in T> : ICommandPlayer<T> where T : IEntityCommand
    {
        Entity Entity { get; }

        bool ICommandPlayer<T>.CanPlay(T command)
        {
            return command.Target == Entity;
        }
    }
}