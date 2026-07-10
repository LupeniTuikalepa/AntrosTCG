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

        public Entity Target => new Entity(sourceEntityId);

        protected EntityCommand(EntityAddress address)
        {
            sourceEntityId = address.entity;
        }

        public EntityAddress TargetEntityAddress(World world) => new EntityAddress(world, Target);

    }
}