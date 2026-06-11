using System.Collections.Generic;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours
{
    public class CapacityDataMapper<TBehaviour, TData>
        where TData : ICapacityBehaviourData
    {
        private readonly List<IBehaviourContainer> containers;


        public CapacityDataMapper()
        {
            containers = new List<IBehaviourContainer>();
        }

        public void Add<T, TB>() where TB : TBehaviour, new() where T : TData
        {
            TB instance = new();
            Add<T>(instance);
        }

        public void Add<T>(TBehaviour behaviour) where T : TData
        {
            var container = new BehaviourContainer<T>(behaviour);
            containers.Add(container);
        }

        public bool TryGetFor<T>(TData data, out T behaviour) 
        {
            foreach (IBehaviourContainer container in containers)
                if (container.Accepts(data) && container.Behaviour is T t)
                {
                    behaviour = t;
                    return true;
                }

            behaviour = default;
            return false;
        }

        private interface IBehaviourContainer
        {
            public TBehaviour Behaviour { get; }
            public bool Accepts(TData data);
        }

        private sealed class BehaviourContainer<T> : IBehaviourContainer where T : TData
        {
            public BehaviourContainer(TBehaviour behaviour)
            {
                Behaviour = behaviour;
            }

            public TBehaviour Behaviour { get; }

            public bool Accepts(TData data)
            {
                return data is T;
            }
        }
    }
}