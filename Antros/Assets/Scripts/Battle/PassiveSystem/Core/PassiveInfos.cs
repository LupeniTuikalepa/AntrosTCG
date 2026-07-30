using ATCG.Battle.Commands.Infos;
using ATCG.Passives.Datas;

namespace ATCG.Battle.PassiveSystem.Core
{
    public struct PassiveInfos : ICommandInfos
    {
        public readonly PassiveData data;

        public PassiveInfos(PassiveData data)
        {
            this.data = data;
        }
    }
}