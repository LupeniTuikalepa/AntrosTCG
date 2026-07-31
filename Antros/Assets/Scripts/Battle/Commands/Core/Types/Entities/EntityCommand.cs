using System;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using UnityEngine;

namespace ATCG.Battle.Commands.Entities
{
    [Serializable]
    public abstract class EntityCommand<T> : Command<T>, IEntityCommand
        where T : struct, ICommandInfos
    {
        [SerializeField]
        private int sourceEntityId;
        [SerializeField]
        private int sourceEntityGeneration;

        // Commands can be queued and processed later than the frame they were built on,
        // which is exactly the kind of gap a recycled entity id can slip through in.
        // Carrying the generation the target had when the command was created means
        // TargetEntityAddress(world).IsAlive-style checks downstream correctly see a
        // stale command as targeting a dead entity instead of whatever new one now
        // occupies the same id.
        public Entity Target => new Entity(sourceEntityId, sourceEntityGeneration);

        protected EntityCommand(EntityAddress address, string source = "None") : base(source)
        {
            sourceEntityId = address.entity;
            sourceEntityGeneration = address.entity.generation;
        }
        
        public EntityAddress TargetEntityAddress(World world) => new EntityAddress(world, Target);

    }
}