using System;

namespace Helteix.Tools.DataMapping
{
    /// <summary>
    /// Generic container: the behaviour is stored in its CONCRETE type, so a
    /// struct lives inline and is never boxed. The reference held by the mapper
    /// is this container (a class), not the behavior.
    /// </summary>
    public abstract class Container<TData, TBehaviour> : IContainer<TData>
        where TData : IData
        where TBehaviour : IBehaviour<TData>
    {
        protected readonly TBehaviour behaviour; // concrete type → inline, no box
        protected Container(TBehaviour behaviour) => this.behaviour = behaviour;
        public Type DataType => typeof(TData);
    }
}