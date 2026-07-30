using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Passives.Datas;
using Helteix.Tools.DataMapping;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.PassiveSystem.Core
{
    public readonly struct PassiveContainerComponent : IEntityComponent
    {
        private readonly int capacity;
        private readonly BattlePhase battlePhase;
        private readonly Dictionary<PassiveData, PassiveListenerGroup> groups;

        public PassiveContainerComponent(int capacity, BattlePhase battlePhase)
        {
            this.capacity = capacity;
            this.battlePhase = battlePhase;
            groups = DictionaryPool<PassiveData, PassiveListenerGroup>.Get();
        }

        public void AddPassive(PassiveData data, PassiveContext ctx)
        {
            if(groups.Count + 1 > capacity)
                return;

            if (!data.TryGet(out IPassiveContainer container))
                return;
            
            var list = ListPool<IPassiveCommandListener>.Get();
            list.AddRange(container.GetListeners(data, ctx));
                    
            var listenerGroup = new PassiveListenerGroup(list);
            listenerGroup.Connect();
                
            groups[data] = listenerGroup;
                
            var applyPassiveCommand = new ApplyPassiveCommand(ctx.owner, ctx);
            applyPassiveCommand.Schedule(ctx.battlePhase);
        }

        public void RemovePassive(PassiveData data, PassiveContext ctx)
        {
            if (!groups.TryGetValue(data, out var listenerGroup)) 
                return;
            
            listenerGroup.Disconnect();
            groups.Remove(data);
            
            var removePassiveCommand = new RemovePassiveCommand(ctx.owner, ctx);
            removePassiveCommand.Schedule(ctx.battlePhase);
        }
        
        public void RemoveAllPassive(EntityAddress owner)
        {
            foreach (var (data, group) in groups)
            {
                var ctx = new PassiveContext(owner, battlePhase, data);
                RemovePassive(data, ctx);
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