using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Passives.Datas;

namespace ATCG.Battle.PassiveSystem.Core
{
    public struct PassiveContext
    {
        private interface IPassiveProperty{}
        
        private class PassiveProperty<T> : IPassiveProperty
        {
            public readonly T value;

            public PassiveProperty(T value)
            {
                this.value = value;
            }
        } 
        
        private readonly Dictionary<string, IPassiveProperty> bag;

        public readonly BattlePhase battlePhase;
        public readonly EntityAddress target;
        public readonly PassiveData data;


        public PassiveContext(EntityAddress target, BattlePhase battlePhase, PassiveData data)
        {
            this.target = target;
            this.battlePhase = battlePhase;
            this.data = data;
            bag = new Dictionary<string, IPassiveProperty>();
        }

        public void AddProperty<T>(string key, T value)
        {
            bag[key] = new PassiveProperty<T>(value);
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (bag.TryGetValue(key, out var property) && property is PassiveProperty<T> passiveProperty)
            {
                value = passiveProperty.value;
                return true;
            }
            
            value = default;
            return false;
        }
    }

    
}