using ATCG.Battle.Entities;

namespace ATCG.Battle.Commands.Entities
{
    public interface IEntityCommand : ICommand
    {
        Entity Target { get; }
    }
}