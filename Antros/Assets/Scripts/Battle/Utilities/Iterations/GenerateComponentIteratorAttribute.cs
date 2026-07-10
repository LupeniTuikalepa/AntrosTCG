using System;

namespace ATCG.Battle.Utilities.Iterations
{
    /// <summary>
    /// Like <see cref="GenerateIteratorAttribute"/>, but for interfaces whose implementors
    /// are ECS components: the generated iterator's Process&lt;T&gt;() keeps the
    /// struct, IEntityComponent constraint so it stays usable with World.Query,
    /// EntityAddress.TryGetComponent and the rest of the sparse-set storage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateComponentIteratorAttribute : Attribute
    {

    }
}
