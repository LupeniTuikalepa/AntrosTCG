namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public interface IEntitySelectionController
    {
        int MaxSelectableEntities { get; }

        

        void OnSelectedEntity(RuntimeEntityManager manager, IRuntimeEntity runtimeEntity);

        void OnDeselectedEntity(RuntimeEntityManager manager, IRuntimeEntity runtimeEntity);
    }
}