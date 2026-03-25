using System.Collections.Generic;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Patterns
{
    public class CapacityBehaviourContainer<TBehaviour, TData>
        where TBehaviour : ICapacityBehaviour<TData>
        where TData : ICapacityBehaviourData
    {
        private interface IBehaviourContainer
        {
            public TBehaviour Behaviour { get; }
            public bool Accepts(TData data);
        }

        private sealed class BehaviourContainer<T> : IBehaviourContainer where T : TData
        {
            public TBehaviour Behaviour { get; }

            public BehaviourContainer(TBehaviour behaviour)
            {
                Behaviour = behaviour;
            }

            public bool Accepts(TData data) => data is T;
        }


        private readonly List<IBehaviourContainer> containers;


        public CapacityBehaviourContainer()
        {
            containers = new();
        }

        public void Add<T, TB>() where TB : TBehaviour, new() where T : TData
        {
            TB instance = new();
            Add<T>(instance);
        }

        public void Add<T>(TBehaviour behaviour) where T : TData
        {
            BehaviourContainer<T> container = new BehaviourContainer<T>(behaviour);
            containers.Add(container);
        }

        public bool TryGetFor<T>(TData data, out T behaviour) where T : ICapacityBehaviour<TData>
        {
            foreach (IBehaviourContainer container in containers)
            {
                if (container.Accepts(data) && container.Behaviour is T t)
                {
                    behaviour = t;
                    return true;
                }
            }

            behaviour = default;
            return false;
        }

    }
}