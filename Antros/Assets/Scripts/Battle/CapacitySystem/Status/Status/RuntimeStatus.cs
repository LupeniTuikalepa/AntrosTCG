using System;
using ATCG.Battle.CapacitySystem.Status.Status.Components;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Status
{
    public class RuntimeStatus : MonoBehaviour
    {

        private IRuntimeStatusComponent[] components;

        protected virtual void Awake()
        {
            components = GetComponentsInChildren<IRuntimeStatusComponent>();
        }
        
        public void Apply(RuntimeStatusContext context)
        {
            for (int i = 0; i < components.Length; i++)
                RunSafely(components[i], c => c.OnApplyStatus(context), "OnApplyStatus");
        }

        // Remove is the one call site that matters most to guard: it runs right before
        // RuntimeEntity destroys this whole GameObject (see StatusRemoveCommand.Play). One
        // component throwing here (as PropagateMaterialOnRenderers used to, reading a
        // just-nulled reference) used to abort the loop AND propagate past Destroy(), so
        // the entire RuntimeStatus — every VFX/audio/material component on it, not just
        // the one that threw — never got cleaned up even though the status had already
        // left on the ECS side. Every component now gets its chance to clean up
        // regardless of a sibling's failure, and a thrown exception no longer stops the
        // caller from destroying this object afterward.
        public void Remove(RuntimeStatusContext context)
        {
            for (int i = 0; i < components.Length; i++)
                RunSafely(components[i], c => c.OnRemoveStatus(context), "OnRemoveStatus");
        }

        public void Tick(RuntimeStatusContext context)
        {
            for (int i = 0; i < components.Length; i++)
                RunSafely(components[i], c => c.OnTickStatus(context), "OnTickStatus");
        }

        private void RunSafely(IRuntimeStatusComponent component, Action<IRuntimeStatusComponent> call, string phase)
        {
            try
            {
                call(component);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                Debug.LogError($"[RuntimeStatus] {phase} threw on '{component.GetType().Name}' " +
                                $"(gameObject '{gameObject.name}') — continuing with the remaining components.", this);
            }
        }
    }
}