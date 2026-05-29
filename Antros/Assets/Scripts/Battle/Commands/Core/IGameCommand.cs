using System.Collections.Generic;

namespace ATCG.Battle.Commands.Core
{
    public interface IGameCommand
    {
        void Process(in GameCommandContext context);
        IReadOnlyList<GameCommand> Embeds { get; }
        GameCommand Parent { get; }
        IEnumerable<GameCommand> GetChildren();
        IEnumerable<T> GetChildren<T>() where T : GameCommand;
        bool HasAnyChildrenOfType<T>(out T firstFound);
        bool HasAnyAncestorOfType<T>(out T firstFound) where T : GameCommand;
        IEnumerable<T> GetAncestorsOfType<T>() where T : GameCommand;
        IEnumerable<GameCommand> GetAncestors();
    }
}