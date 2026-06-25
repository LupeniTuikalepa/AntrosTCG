namespace ATCG.Battle.Entities.Components.Status
{
    public readonly struct StatusInfos<TStatus> : IEntityComponent where TStatus : struct, IStatusComponent
    {
        public readonly ComponentMask componentMask;

        public StatusInfos(ComponentMask componentMask)
        {
            this.componentMask = componentMask;
        }
    }
}