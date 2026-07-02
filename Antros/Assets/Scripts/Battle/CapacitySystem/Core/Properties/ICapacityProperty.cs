namespace ATCG.Battle.CapacitySystem.Core.Properties
{
    public interface ICapacityProperty
    {
    }

    public class CapacityProperty<T> : ICapacityProperty
    {
        public T Value { get; private set; }
        public CapacityProperty(T value)
        {
            Value = value;
        }
    }
}