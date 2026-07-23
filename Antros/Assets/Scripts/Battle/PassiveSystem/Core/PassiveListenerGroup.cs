using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Passives.Datas;

namespace ATCG.Battle.PassiveSystem.Core
{
    public readonly struct PassiveListenerGroup
    {
        public readonly List<IPassiveCommandListener> listeners;

        public PassiveListenerGroup(List<IPassiveCommandListener> listeners)
        {
            this.listeners = listeners;
        }
        
        public void Connect()
        {
            foreach (var listener in listeners)
            {
                listener.Register();
            }
        }

        public void Disconnect()
        {
            foreach (var listener in listeners)
            {
                listener.Unregister();
            }
        }
    }
}