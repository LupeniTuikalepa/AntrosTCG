using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Passives.Datas;
using Helteix.Tools.DataMapping;
using UnityEngine.Pool;

namespace ATCG.Battle.PassiveSystem.Core
{
    public readonly struct PassiveContainerComponent : IEntityComponent
    {
        private readonly int capacity;
        private readonly Dictionary<PassiveData, PassiveListenerGroup> groups;

        public PassiveContainerComponent(int capacity)
        {
            this.capacity = capacity;
            groups = DictionaryPool<PassiveData, PassiveListenerGroup>.Get();
        }

        public void AddPassive(PassiveData data, PassiveContext ctx)
        {
            if(groups.Count + 1 > capacity)
                return;
            
            if (data.TryGet(out IPassiveContainer container))
            {
                var list = ListPool<IPassiveCommandListener>.Get();
                list.AddRange(container.GetListeners(data, ctx));
                    
                var listenerGroup = new PassiveListenerGroup(list);
                listenerGroup.Connect();
                
                groups[data] = listenerGroup;
            }
        }

        public void RemovePassive(PassiveData data)
        {
            if (!groups.TryGetValue(data, out var listenerGroup)) 
                return;
            
            listenerGroup.Disconnect();
            groups.Remove(data);
        }

        void IEntityComponent.Dispose()
        {
            foreach (var (key, value) in groups)
            {
                value.Disconnect();
                ListPool<IPassiveCommandListener>.Release(value.listeners);
            }
            
            DictionaryPool<PassiveData, PassiveListenerGroup>.Release(groups);
        }
    }
}