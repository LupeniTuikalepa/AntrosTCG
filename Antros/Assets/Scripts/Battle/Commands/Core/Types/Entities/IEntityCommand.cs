using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Core
{
    public interface IEntityCommand : ICommand
    {
        Entity Target { get; }
    }
}