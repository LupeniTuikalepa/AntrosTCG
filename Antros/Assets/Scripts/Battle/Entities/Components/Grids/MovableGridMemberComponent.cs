using Helteix.ChanneledProperties.Formulas;

namespace ATCG.Battle.Entities.Components.Tags
{
    public struct MovableGridMemberComponent : IEntityComponent
    {
        public readonly Formula<int> speed;

        public MovableGridMemberComponent(Formula<int> speed)
        {
            this.speed = speed;
        }
    }
}