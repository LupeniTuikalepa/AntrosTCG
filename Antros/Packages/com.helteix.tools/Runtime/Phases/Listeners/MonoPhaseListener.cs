using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Helteix.Tools.Phases.Listeners
{
    public abstract class MonoPhaseListener<T> : MonoBehaviour, IPhaseListener<T> where T : IPhase
    {
        [field: InspectorName("OnPhaseBegin"), SerializeField]
        public UnityEvent<T> OnPhaseBeginUnityEvent { get; private set; }

        [field: InspectorName("OnPhaseEnd"), SerializeField]
        public UnityEvent<T> OnPhaseEndUnityEvent { get; private set; }

        public event Action<T> OnPhaseBegins;
        public event Action<T> OnPhaseEnds;

        protected virtual void OnEnable()
        {
            this.Register();
        }

        protected virtual void OnDisable()
        {
            this.Unregister();
        }

        void IPhaseListener<T>.OnPhaseBegin(T phase)
        {
            OnPhaseBegin(phase);

            OnPhaseBeginUnityEvent.Invoke(phase);
            OnPhaseBegins?.Invoke(phase);
        }

        void IPhaseListener<T>.OnPhaseEnd(T phase)
        {
            OnPhaseEnd(phase);

            OnPhaseEndUnityEvent.Invoke(phase);
            OnPhaseEnds?.Invoke(phase);
        }

        protected virtual void OnPhaseBegin(T phase) { }
        protected virtual void OnPhaseEnd(T phase) { }

    }
}