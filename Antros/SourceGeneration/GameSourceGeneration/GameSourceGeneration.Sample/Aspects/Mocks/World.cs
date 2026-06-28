namespace ATCG.Battle.Entities;

public struct World
{
    public Entity CreateEntity() => new Entity();

    public void AddComponent<T>(Entity entity, T component = default) { }
}