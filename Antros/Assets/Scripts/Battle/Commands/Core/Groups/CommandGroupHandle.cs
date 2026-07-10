using System;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Commands.Groups
{
    public readonly struct CommandGroupHandle : IDisposable
    {
        private readonly BattleID id;

        public CommandGroupHandle(BattleID id)
        {
            this.id = id;
        }

        void IDisposable.Dispose()
        {
            CommandManager.EndGroup(id);
        }
    }
}