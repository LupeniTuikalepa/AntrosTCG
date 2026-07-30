namespace ATCG.Battle.PassiveSystem.Runtimes
{
    public class EmptyRuntimePassive : RuntimePassive
    {
        public override void Apply(RuntimePassiveContext context)
        {
        }

        public override void Remove(RuntimePassiveContext context)
        {
            Destroy(gameObject);
        }

        public override void Tick(RuntimePassiveContext context)
        {
        }
    }
}