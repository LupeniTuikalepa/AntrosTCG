using System;
using ATCG.Battle.PassiveSystem.Runtimes.Components;
using UnityEngine;

namespace ATCG.Battle.PassiveSystem.Runtimes
{
    //passer par un mono oblige de faire un dico dans le runtimeEntity
    public class RuntimePassive : MonoBehaviour
    {
        private IRuntimePassiveComponent[] components;

        private void Awake()
        {
            components = GetComponentsInChildren<IRuntimePassiveComponent>();
        }
        
        public void Apply(RuntimePassiveContext context)
        {
            for (int i = 0; i < components.Length; i++)
                RunSafely(components[i], c => c.OnApplyPassive(context), "OnApplyStatus");
        }

        public void Remove(RuntimePassiveContext context)
        {
            for (int i = 0; i < components.Length; i++)
                RunSafely(components[i], c => c.OnRemovePassive(context), "OnRemoveStatus");
        }

        public void Tick(RuntimePassiveContext context)
        {
            for (int i = 0; i < components.Length; i++)
                RunSafely(components[i], c => c.OnTickPassive(context), "OnTickStatus");
        }

        private void RunSafely(IRuntimePassiveComponent component, Action<IRuntimePassiveComponent> call, string phase)
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