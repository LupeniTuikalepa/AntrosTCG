using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using UnityEngine;

namespace ATCG.Battle.Entities.Components.Implementations
{
    public readonly struct PoisonStatusComponent : IStatusComponent
    {
        private readonly int amount;

        public PoisonStatusComponent(int amount)
        {
            this.amount = amount;
        }

        public void Trigger(EntityAddress address, BattlePhase battlePhase)
        {
            var damageCommand = new DamageCommand(amount, address);
            damageCommand.Run(battlePhase);
        }
    }
}