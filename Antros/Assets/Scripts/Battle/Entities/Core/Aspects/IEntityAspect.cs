using System;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities
{
    public interface IEntityAspect : IEquatable<IEntityAspect>
    {
        bool IsValid
        {
            get
            {
                ComponentMask mask = ComponentMask;
                return EntityAddress.IsValid && EntityAddress.HasAllComponents(in mask);
            }
        }

        ComponentMask ComponentMask => default;

        EntityAddress EntityAddress { get; set; }

        bool IEquatable<IEntityAspect>.Equals(IEntityAspect aspect)
        {
            return aspect != null && aspect.EntityAddress == EntityAddress;
        }
    }

    public interface IEntityAspect<T1> : IEntityAspect
        where T1 : struct, IEntityComponent
    {
        ComponentMask IEntityAspect.ComponentMask => ComponentMask
            .With<T1>();
    }

    public interface IEntityAspect<T1, T2> : IEntityAspect
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
    {
        ComponentMask IEntityAspect.ComponentMask => ComponentMask
            .With<T1>()
            .With<T2>();
    }

    public interface IEntityAspect<T1, T2, T3> : IEntityAspect
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
    {
        ComponentMask IEntityAspect.ComponentMask => ComponentMask
            .With<T1>()
            .With<T2>()
            .With<T3>();
    }

    public interface IEntityAspect<T1, T2, T3, T4> : IEntityAspect
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
        where T4 : struct, IEntityComponent
    {
        ComponentMask IEntityAspect.ComponentMask => ComponentMask
            .With<T1>()
            .With<T2>()
            .With<T3>()
            .With<T4>();
    }

    public interface IEntityAspect<T1, T2, T3, T4, T5> : IEntityAspect
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
        where T4 : struct, IEntityComponent
        where T5 : struct, IEntityComponent
    {
        ComponentMask IEntityAspect.ComponentMask => ComponentMask
            .With<T1>()
            .With<T2>()
            .With<T3>()
            .With<T4>()
            .With<T5>();
    }

    public interface IEntityAspect<T1, T2, T3, T4, T5, T6> : IEntityAspect
        where T1 : struct, IEntityComponent
        where T2 : struct, IEntityComponent
        where T3 : struct, IEntityComponent
        where T4 : struct, IEntityComponent
        where T5 : struct, IEntityComponent
        where T6 : struct, IEntityComponent
    {
        ComponentMask IEntityAspect.ComponentMask => ComponentMask
            .With<T1>()
            .With<T2>()
            .With<T3>()
            .With<T4>()
            .With<T5>()
            .With<T6>();
    }
}