using UnityEngine;

namespace Helteix.Tools.SerializedComponent.Containers
{

    [System.Serializable]
    public struct SComponentContainer<T> where T : class, ISComponent
    {
        [field: SerializeReference]
        public T Component { get; private set; }

        public bool HasComponent => Component != null;

        public SComponentContainer(T component)
        {
            this.Component = component;
        }


        public void SetComponent(T component)
        {
            this.Component = component;
        }

        public void Clear() => SetComponent(null);

        public static implicit operator bool(SComponentContainer<T> container) => container.HasComponent;
    }
}