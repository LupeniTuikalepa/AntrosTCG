using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Core
{
    public interface IEntityCommand : IGameCommand
    {
        Entity Target { get; }
    }
}