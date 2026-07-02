using System;
using ATCG.Capacities.Data.Status;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatus : MonoBehaviour
    {
        private event Action<RuntimeStatusContext> OnApplyStatus;
        private event Action<RuntimeStatusContext> OnRemoveStatus;
        private event Action<RuntimeStatusContext> OnTickStatus;
        
        private IRuntimeStatusComponent[] components; 
        
        protected virtual void Awake()
        {
            components = GetComponentsInChildren<IRuntimeStatusComponent>();
        }

        private void OnEnable()
        {
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                OnApplyStatus += component.OnApplyStatus;
                OnRemoveStatus += component.OnRemoveStatus;
                OnTickStatus += component.OnTickStatus;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                OnApplyStatus -= component.OnApplyStatus;
                OnRemoveStatus -= component.OnRemoveStatus;
                OnTickStatus -= component.OnTickStatus;
            }
        }
        
        public void Apply(RuntimeStatusContext context)
        {
            OnApplyStatus?.Invoke(context);
        }

        public void Remove(RuntimeStatusContext context)
        {
            OnRemoveStatus?.Invoke(context);
        }
        
        public void Tick(RuntimeStatusContext context)
        {
            OnTickStatus?.Invoke(context);
        }
    }
}