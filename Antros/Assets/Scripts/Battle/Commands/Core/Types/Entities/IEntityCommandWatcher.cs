using ATCG.Battle.Commands.Watchers;
using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Entities
{
    public interface IEntityCommandWatcher<in T> : ICommandWatcher<T> where T : IEntityCommand
    {
        Entity Target { get; }

        bool ICommandWatcher<T>.Accepts(T command) => command.Target == Target;
    }
}