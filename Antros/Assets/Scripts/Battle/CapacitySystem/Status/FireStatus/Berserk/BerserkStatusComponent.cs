using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Watchers;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Berserk
{
    public struct BerserkStatusComponent : IStatusComponent
    {
        public class BerserkWatcher : IEntityCommandWatcher<DamageCommand>, IEntityCommandWatcher<BasicAttackCommand>
        {
            public Entity Target { get; }

            private ComponentRef<StatusVolatileController> volatileControllerRef;

            public BerserkWatcher(Entity target, ComponentRef<StatusVolatileController> volatileControllerRef)
            {
                Target = target;
                this.volatileControllerRef = volatileControllerRef;
            }

            void ICommandWatcher<DamageCommand>.Trigger(DamageCommand command) => Trigger();
            void ICommandWatcher<BasicAttackCommand>.Trigger(BasicAttackCommand command) => Trigger();

            private void Trigger()
            {
                volatileControllerRef.GetValue().Trigger();
            }
        }

        public readonly ChannelKey channelKey;
        public StatusData StatusData { get; }
        public BerserkWatcher Watcher { get; private set; }

        public BerserkStatusComponent(BerserkStatusData data, ChannelKey channelKey)
        {
            StatusData = data;
            this.channelKey = channelKey;
            Watcher = null;
        }

        public void Watch(EntityAddress target, ComponentRef<StatusVolatileController> volatileControllerRef)
        {
            Watcher?.UnregisterWatcher();
            Watcher = new BerserkWatcher(target, volatileControllerRef);
            Watcher.RegisterWatcher();
        }

        void IEntityComponent.Dispose()
        {
            Watcher?.UnregisterWatcher();
        }
    }
}