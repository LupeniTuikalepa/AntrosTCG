using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
using ATCG.Databases;
using UnityEngine;

namespace ATCG.Battle.Commands.Listeners
{
    public abstract class MonoEntitySignalListener : 
        MonoBaseSignalListener<EntityCommandSignal>, 
        IEntitySignalListener
    {
        public Entity Target { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Target = RuntimeEntity.Address.entity;
        }

        public abstract override void Trigger(CommandContext context, EntityCommandSignal command);
    }
}