using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ATCG.Battle.Entities.Runtime
{
    public interface IRuntimeEntity
    {
        GameObject gameObject { get; }

        EntityAddress Address { get; }
        void OnSelected();
        void OnDeselected();
        void SetInteractableState(bool isInMask);
    }
}