namespace ATCG.Battle.Entities.Components.Status
{
    public interface IStatusController<T> : IEntityComponent where T : struct, IStatus
    {
        public bool IsFinished(ComponentRef<T> componentRef);

    }
}