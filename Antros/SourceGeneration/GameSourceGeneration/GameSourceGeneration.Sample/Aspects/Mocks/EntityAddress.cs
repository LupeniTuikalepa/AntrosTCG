namespace ATCG.Battle.Entities;

public struct EntityAddress
{
    public Entity entity;
    public World world;

    public EntityAddress(World world, Entity entity)
    {
        this.world = world;
        this.entity = entity;
    }

    public ref T GetComponent<T>() where T : new()
    {
        return ref RefProvider<T>.Get();
    }

    public void AddComponent<T>(T t) where T : new()
    {

    }

    public bool HasComponent<T>() where T : new()
        => true;
}

public static class RefProvider<T>
{
    private static T component;

    public static ref T Get() => ref component;
}