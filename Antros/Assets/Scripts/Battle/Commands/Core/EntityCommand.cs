using System;
using ATCG.Battle.Entities;
using UnityEngine;

namespace ATCG.Battle.Commands.Core
{
    [Serializable]
    public abstract class EntityCommand : GameCommand
    {
        [SerializeField]
        private int targetEntityId;

        public Entity TargetEntity => new Entity(targetEntityId);

        protected EntityCommand(Entity targetEntity)
        {
            targetEntityId = targetEntity;
        }

        public EntityAddress TargetEntityAddress(World world) => new EntityAddress(world, TargetEntity);
    }
}