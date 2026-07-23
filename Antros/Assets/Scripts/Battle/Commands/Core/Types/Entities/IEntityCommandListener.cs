using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Entities
{
    public interface IEntityCommandListener<in T> : ICommandListener<T> where T : IEntityCommand
    {
        Entity Target { get; }

        bool ICommandListener<T>.Accepts(CommandContext context, T command) => command.Target == Target;
    }
}