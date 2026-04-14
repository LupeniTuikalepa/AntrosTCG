namespace ATCG.Battle.Entities.Components
{
    public struct ComponentID<T> where T : IEntityComponent
    {
        // ReSharper disable once StaticMemberInGenericType
        public static readonly int ID = ComponentRegistry.Next<T>();
    }
}