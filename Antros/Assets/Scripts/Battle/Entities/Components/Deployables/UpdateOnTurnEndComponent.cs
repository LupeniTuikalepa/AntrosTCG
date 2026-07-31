using System;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Listeners;

namespace ATCG.Battle.Entities.Components.Deployables
{
    public struct UpdateOnTurnEndComponent : IEntityComponent
    {
        private class Wrapper : ICommandListener<EndTurnCommand>
        {
            private readonly Action action;

            public Wrapper(Action action)
            {
                this.action = action;
            }
            public void Trigger(CommandContext context, EndTurnCommand command)
            {
                action();
            }
        }

        private readonly Action action;
        private Wrapper wrapper;
        
        public UpdateOnTurnEndComponent(Action action)
        {
            this.action = action;
            wrapper = new Wrapper(action);
        }
    }
}