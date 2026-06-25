using System;

namespace Helteix.Tools.DataMapping
{
    /// <summary>
    /// Marks a method on a <c>[GenerateContainer]</c> behaviour interface as one
    /// that should be exposed on the generated container.
    /// <para>
    /// Only methods carrying this attribute are surfaced on the container
    /// interface and forwarded by the concrete container; every other method on
    /// the behaviour interface (helpers, default-interface-method logic such as a
    /// pattern's <c>GetAll</c>) stays internal to the behaviour and is neither
    /// exposed nor validated.
    /// </para>
    /// <para>
    /// A marked method must take the behaviour's data type as its first parameter
    /// (so the container can widen it to the data root and downcast). Violating
    /// that is a compile error (HTX002). Unmarked methods are free to take any
    /// signature.
    /// </para>
    /// <para>
    /// Note on boxing: when a marked method is a default interface method whose
    /// body re-invokes other interface members on <c>this</c>, those inner calls
    /// run against <c>this</c> typed as the interface. For a struct behaviour that
    /// reboxes per call. The direct forward from the container is still unboxed;
    /// only DIM bodies that call back into the interface pay this cost.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AddToContainerAttribute : Attribute
    {
    }
}