using ATCG.Battle.CapacitySystem.Core;
using ATCG.Passives.Datas;
using Helteix.Tools.DataMapping;
using Helteix.Tools.Phases;

namespace ATCG.Battle.PassiveSystem.Core
{
    public interface IPassive<in T> : IBehaviour<T> where T : PassiveData
    {
    }
}