using System;
using ATCG.Battle.Entities.Runtime;
using ATCG.Databases;
using UnityEngine;

namespace ATCG.Battle.Commands.Listeners
{
    public abstract class MonoBaseSignalListener<T> : MonoBehaviour, IBaseSignalListener<T> where T : ICommandSignal
    {
        [field: SerializeField]
        public GameDatabaseObject[] Sources { get; private set; }
        
        public IRuntimeEntity RuntimeEntity { get; private set; }

        protected virtual void Awake()
        {
            if(TryGetComponent<IRuntimeEntity>(out var entity))
                RuntimeEntity = entity;
        }

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }
        
        public abstract void Trigger(CommandContext context, T command);
    }
}