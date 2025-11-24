namespace ATCG.Capacities
{
    public interface ICapacityDescriptions
    {
        public int Cost { get; }
        public string Name { get; }
        public string Description { get; }
        bool IsValid { get; }
    }
}