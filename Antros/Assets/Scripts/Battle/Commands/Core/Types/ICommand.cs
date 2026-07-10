using System.Collections.Generic;

namespace ATCG.Battle.Commands
{
    public interface ICommand : IBaseCommand
    {
        void Process(in CommandContext context);

        IReadOnlyList<BattleID> Embeds { get; }
        BattleID Parent { get; }
        BattleID ID { get; }
        void SetParent(ICommand parent);
    }
}