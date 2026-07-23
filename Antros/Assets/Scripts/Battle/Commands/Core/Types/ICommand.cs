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

        public void Inject<TCommand>(in CommandContext context)
            where TCommand : ICommand, new();

        public void Inject<TCommand>(in CommandContext context, TCommand command)
            where TCommand : ICommand;
    }
}