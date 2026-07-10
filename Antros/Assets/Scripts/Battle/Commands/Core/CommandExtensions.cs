using System.Collections.Generic;

namespace ATCG.Battle.Commands
{
    public static class CommandExtensions
    {

        public static IEnumerable<ICommand> GetEmbeds(this ICommand command, CommandContext context)
        {
            foreach (var embed in command.Embeds)
            {
                if(context.TryGetCommand(embed, out ICommand c))
                    yield return c;
            }
        }
        public static IEnumerable<ICommand> GetChildren(this ICommand command, CommandContext context)
        {
            foreach (var children in command.Embeds)
            {
                if (!context.TryGetCommand(children, out ICommand c))
                    continue;

                yield return c;

                foreach (ICommand childrenEmbed in c.GetChildren(context))
                    yield return childrenEmbed;
            }
        }

        public static IEnumerable<TCommand> GetChildren<TCommand>(this ICommand command, CommandContext context) where TCommand : ICommand
        {
            foreach (ICommand subEvent in GetChildren(command, context))
                if (subEvent is TCommand t)
                    yield return t;
        }

        public static bool HasAnyChildrenOfType<TCommand>(this ICommand command, out TCommand firstFound, CommandContext context) where TCommand : ICommand
        {
            foreach (ICommand subEvent in GetChildren(command, context))
                if (subEvent is TCommand t)
                {
                    firstFound = t;
                    return true;
                }

            firstFound = default;
            return false;
        }

        public static bool HasAnyAncestorOfType<TCommand>(this ICommand command, CommandContext context, out TCommand firstFound) where TCommand : ICommand
        {
            foreach (ICommand e in GetAncestors(command, context))
                if (e is TCommand t)
                {
                    firstFound = t;
                    return true;
                }

            firstFound = default;
            return false;
        }

        public static IEnumerable<TCommand> GetAncestorsOfType<TCommand>(this ICommand command, CommandContext context) where TCommand : ICommand
        {
            foreach (ICommand entityEvent in GetAncestors(command, context))
                if (entityEvent is TCommand t)
                    yield return t;
        }

        public static IEnumerable<ICommand> GetAncestors(this ICommand command, CommandContext context)
        {
            ICommand parent = command;
            while (parent != null && !context.IsRoot(parent))
            {
                if (context.TryGetCommand(command.Parent, out ICommand c))
                {
                    yield return c;
                    parent = c;
                }
            }
        }
    }
}