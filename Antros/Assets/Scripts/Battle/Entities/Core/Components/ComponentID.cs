namespace ATCG.Battle.Entities.Core.Components
{
    public struct ComponentID<T> where T : IEntityComponent
    {
        // ReSharper disable once StaticMemberInGenericType
        public static readonly int ID = ComponentRegistry.Next();
    }
}