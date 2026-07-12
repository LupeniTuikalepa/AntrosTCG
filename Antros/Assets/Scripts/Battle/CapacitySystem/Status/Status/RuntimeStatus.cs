using System;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatus : MonoBehaviour
    {

        private IRuntimeStatusComponent[] components;

        protected virtual void Awake()
        {
            components = GetComponentsInChildren<IRuntimeStatusComponent>();
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
            }
        }

        public void Apply(RuntimeStatusContext context)
        {
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                component.OnApplyStatus(context);
            }
        }

        public void Remove(RuntimeStatusContext context)
        {
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                component.OnRemoveStatus(context);
            }
        }

        public void Tick(RuntimeStatusContext context)
        {
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                component.OnTickStatus(context);
            }
        }
    }
}