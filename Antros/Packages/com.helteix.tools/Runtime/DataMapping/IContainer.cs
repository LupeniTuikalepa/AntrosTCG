using System;

namespace Helteix.Tools.DataMapping
{
    /// <summary>
    /// Non-generic container handle. Never exposes the behaviour itself —
    /// that is what keeps a struct behaviour from being boxed. Domains derive a
    /// richer non-generic interface with business ops that use the behaviour
    /// internally.
    /// </summary>
    public interface IContainer<in TData> where TData : IData
    {
        Type DataType { get; }
    }
}