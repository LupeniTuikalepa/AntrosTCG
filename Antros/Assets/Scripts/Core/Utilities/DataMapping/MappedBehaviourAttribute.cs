using System;

namespace Helteix.Tools.DataMapping
{
    /// <summary>
    /// Placed on a domain's behaviour interface (e.g. <c>IHexPattern&lt;TData&gt;</c>
    /// or <c>ICapacityEffect&lt;TData&gt;</c>). Tells the source generator how to
    /// wire any struct implementing that interface into the mapping system:
    /// it emits the <c>ISelfMapping&lt;TData&gt;</c> partial whose BuildAndRegister
    /// pushes a freshly-built container into the right bucket.
    /// <para>
    /// <paramref name="containerOpenGeneric"/>: the concrete container open
    /// generic, e.g. <c>typeof(PatternContainer&lt;,&gt;)</c>. The generator
    /// closes it as <c>PatternContainer&lt;TData, TBehaviour&gt;</c>.
    /// </para>
    /// <para>
    /// <paramref name="domainBucket"/>: the non-generic domain container interface
    /// used as the bucket key, e.g. <c>typeof(IPatternContainer)</c>.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class MappedBehaviourAttribute : Attribute
    {
        public Type ContainerOpenGeneric { get; }
        public Type DomainBucket { get; }

        public MappedBehaviourAttribute(Type containerOpenGeneric, Type domainBucket)
        {
            ContainerOpenGeneric = containerOpenGeneric;
            DomainBucket = domainBucket;
        }
    }
}