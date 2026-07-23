using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.PassiveSystem.Core
{
    public abstract class Passive<TCommand> :
        ICommandListener<TCommand>,
        ICommandDirector
        where TCommand : ICommand
    {
        public abstract bool Accepts(CommandContext context, TCommand command);

        public void Trigger(CommandContext context, TCommand command)
        {
            //Inject(context, this);
        }
    }
}