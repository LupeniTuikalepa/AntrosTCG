namespace ATCG.Battle.Commands.Infos
{
    public struct DeltaInfos<T> : ICommandInfos
    {
        public T from;
        public T to;
    }
}