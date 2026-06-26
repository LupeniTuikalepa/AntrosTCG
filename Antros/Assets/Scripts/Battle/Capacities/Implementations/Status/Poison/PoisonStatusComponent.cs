using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Components.Implementations
{
    public readonly struct PoisonStatusComponent : IStatusComponent
    {
        private readonly PoisonStatusData data;
        
        private int Amount => data.Damage;
        StatusData IStatusComponent.StatusData => data;
        

        public PoisonStatusComponent(PoisonStatusData data)
        {
            this.data = data;
        }


        public void Trigger(EntityAddress address, BattlePhase battlePhase)
        {
            var damageCommand = new DamageCommand(Amount, address);
            damageCommand.Run(battlePhase);
        }
    }
}