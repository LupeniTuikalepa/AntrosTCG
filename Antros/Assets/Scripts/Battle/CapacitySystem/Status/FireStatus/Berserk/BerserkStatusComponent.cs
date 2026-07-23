using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Berserk
{
    public struct BerserkStatusComponent : IStatusComponent
    {
        public class BerserkListener : IEntityCommandListener<DamageCommand>, IEntityCommandListener<BasicAttackCommand>
        {
            public Entity Target { get; }

            private ComponentRef<StatusVolatileController> volatileControllerRef;

            public BerserkListener(Entity target, ComponentRef<StatusVolatileController> volatileControllerRef)
            {
                Target = target;
                this.volatileControllerRef = volatileControllerRef;
            }

            void ICommandListener<DamageCommand>.Trigger(CommandContext context, DamageCommand command) => Trigger();
            void ICommandListener<BasicAttackCommand>.Trigger(CommandContext context, BasicAttackCommand command) => Trigger();

            private void Trigger()
            {
                volatileControllerRef.GetValue().Trigger();
            }
        }

        public readonly ChannelKey channelKey;
        public StatusData StatusData { get; }
        public BerserkListener Listener { get; private set; }

        public BerserkStatusComponent(BerserkStatusData data, ChannelKey channelKey)
        {
            StatusData = data;
            this.channelKey = channelKey;
            Listener = null;
        }

        public void Watch(EntityAddress target, ComponentRef<StatusVolatileController> volatileControllerRef)
        {
            Listener?.UnregisterWatcher();
            Listener = new BerserkListener(target, volatileControllerRef);
            Listener.RegisterWatcher();
        }

        void IEntityComponent.Dispose()
        {
            Listener?.UnregisterWatcher();
        }
    }
}