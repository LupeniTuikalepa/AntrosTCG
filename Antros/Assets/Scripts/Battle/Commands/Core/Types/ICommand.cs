using System.Collections.Generic;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Commands.Core
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