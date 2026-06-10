using System;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public abstract class EntityActionUIElement : MonoBehaviour
    {
        protected EntityActionUIController Controller { get; private set; }
        public IRuntimeEntity RuntimeEntity => Controller.RuntimeEntity;

        
        protected virtual void Awake()
        {
            Controller = GetComponentInParent<EntityActionUIController>();
        }
    }
}