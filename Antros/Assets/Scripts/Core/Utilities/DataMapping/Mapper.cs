namespace Helteix.Tools.DataMapping
{
    /// <summary>
    /// The single global entry point. No domain is named by the caller: the
    /// behaviour routes itself via <see cref="ISelfMapping{TData}"/>.
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Register a data→behaviour mapping. The struct behaviour is placed into
        /// a concrete-typed container field — never boxed (verified via IL: the
        /// call below compiles to a constrained call on the struct's address).
        /// </summary>
        public static void Register<TData, TBehaviour>(TBehaviour behaviour)
            where TData : IData
            where TBehaviour : IBehaviour<TData>, ISelfMapping<TData>
            => behaviour.BuildAndRegister();

        /// <summary>Convenience for parameterless behaviours.</summary>
        public static void Register<TData, TBehaviour>()
            where TData : IData
            where TBehaviour : IBehaviour<TData>, ISelfMapping<TData>, new()
            => Register<TData, TBehaviour>(new TBehaviour());

        /// <summary>
        /// O(1) dispatch — a single dictionary lookup. On a hot path, cache the
        /// closed generic by calling once; the JIT resolves DomainBucket&lt;T&gt;
        /// per process, not per call.
        /// </summary>
        public static bool TryGet<TContainer>(this IData data, out TContainer container)
            where TContainer : class, IContainer
            => DomainBucket<TContainer>.TryGet(data, out container);
    }
}