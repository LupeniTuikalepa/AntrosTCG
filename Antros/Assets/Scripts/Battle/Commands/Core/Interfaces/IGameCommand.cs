using System.Collections.Generic;

namespace ATCG.Battle.Commands.Core
{
    public interface IGameCommand
    {
        void Process(in CommandContext context);
        IReadOnlyList<IGameCommand> Embeds { get; }
        IGameCommand Parent { get; }
        IEnumerable<IGameCommand> GetChildren();

        void SetParent(IGameCommand parent);

        IEnumerable<TCommand> GetChildren<TCommand>() where TCommand : IGameCommand;
        bool HasAnyChildrenOfType<TCommand>(out TCommand firstFound) where TCommand : IGameCommand;
        bool HasAnyAncestorOfType<TCommand>(out TCommand firstFound) where TCommand : IGameCommand;
        IEnumerable<TCommand> GetAncestorsOfType<TCommand>() where TCommand : IGameCommand;
        IEnumerable<IGameCommand> GetAncestors();
    }
}