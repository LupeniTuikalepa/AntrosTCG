using System.Collections.Generic;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Passives.Datas;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.PassiveSystem.Core
{
    [GenerateContainer]
    public interface IPassive<in TData> : IBehaviour<TData> where TData : PassiveData
    {
        [AddToContainer]
        public IEnumerable<IPassiveCommandListener> GetListeners(TData data, EntityAddress target);
        
        [AddToContainer]
        public void Tick(TData data, PassiveContext ctx);

        [AddToContainer]
        public void Apply(TData data, PassiveContext ctx){}
        
        [AddToContainer]
        public void Remove(TData data, PassiveContext ctx){}
    }
}