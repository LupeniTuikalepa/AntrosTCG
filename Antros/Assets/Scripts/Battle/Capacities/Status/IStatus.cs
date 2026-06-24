namespace ATCG.Battle.Entities.Components.Status
{
    public interface IStatus: IEntityComponent
    {
        void Trigger(EntityAddress address);
    }
}