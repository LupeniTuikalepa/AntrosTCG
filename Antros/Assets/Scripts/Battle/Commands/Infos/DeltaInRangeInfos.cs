namespace ATCG.Battle.Commands.Infos
{
    public struct DeltaInRangeInfos<T>: ICommandInfos
    {
        public T from;
        public T to;
        public T min;
        public T max;
    }
}