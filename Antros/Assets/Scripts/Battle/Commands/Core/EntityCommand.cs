using System;
using ATCG.Battle.Entities;
using UnityEngine;

namespace ATCG.Battle.Commands.Core
{
    [Serializable]
    public abstract class EntityCommand<T> : GameCommand<T> where T : struct
    {
        [SerializeField]
        private int sourceEntityId;

        public Entity TargetEntity => new Entity(sourceEntityId);

        protected EntityCommand(Entity sourceEntity)
        {
            sourceEntityId = sourceEntity;
        }

        public EntityAddress TargetEntityAddress(World world) => new EntityAddress(world, TargetEntity);
    }
}