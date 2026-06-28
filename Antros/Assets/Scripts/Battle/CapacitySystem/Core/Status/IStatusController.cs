namespace ATCG.Battle.Entities.Components.Status
{
    public interface IStatusController<T> : IEntityComponent where T : struct, IStatusComponent
    {
        public bool IsFinished(ComponentRef<T> componentRef);

    }
}