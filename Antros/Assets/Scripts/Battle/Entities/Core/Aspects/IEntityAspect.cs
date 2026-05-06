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

        EntityAddress EntityAddress { get; }

        bool IEquatable<IEntityAspect>.Equals(IEntityAspect aspect)
        {
            return aspect != null && aspect.EntityAddress == EntityAddress;
        }
    }
}