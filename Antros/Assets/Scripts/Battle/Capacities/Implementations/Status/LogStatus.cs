using ATCG.Battle.Entities.Components.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Components.Implementations
{
    public readonly struct LogStatus : IStatus
    {
        public readonly string log;

        public LogStatus(string log)
        {
            this.log = log;
        }

        public void Trigger(EntityAddress address)
        {
            Debug.Log(log);
        }
    }
}