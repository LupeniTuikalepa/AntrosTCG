using ATCG.Passives.Datas;

namespace ATCG.Battle.PassiveSystem.Runtimes
{
    public struct RuntimePassiveContext
    {
        public readonly PassiveData data;

        public RuntimePassiveContext(PassiveData data)
        {
            this.data = data;
        }
    }
}