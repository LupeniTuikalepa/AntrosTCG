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
        private readonly Dictionary<PassiveData, PassiveListenerGroup> groups;

        public PassiveContainerComponent(List<PassiveData> passiveDatas, EntityAddress target)
        {
            groups = DictionaryPool<PassiveData, PassiveListenerGroup>.Get();
            foreach (var data in passiveDatas)
            {
                if (data.TryGet(out IPassiveContainer container))
                {
                    var list = ListPool<IPassiveCommandListener>.Get();
                    list.AddRange(container.GetListeners(data, target));
                    
                    var group = new PassiveListenerGroup(list);
                    group.Connect();

                    groups.Add(data, group);
                }
            }
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