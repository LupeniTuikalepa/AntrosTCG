using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using UnityEngine;

namespace ATCG.Battle.Entities.Components.Implementations
{
    public readonly struct LogStatusComponent : IStatusComponent
    {
        public readonly string log;

        public LogStatusComponent(string log)
        {
            this.log = log;
        }

        public void Trigger(EntityAddress address, BattlePhase battlePhase)
        {
            Debug.Log(log);
        }
    }
}