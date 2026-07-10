using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector.StatusTab
{
    public class InspectorStatusList : EntityInspectorTabElement
    {
        private sealed class CreateIconForStatus : IStatusIterator
        {
            public EntityAddress address;
            public InspectorStatusElement statusElementPrefab;
            public Transform container;

            void IStatusIterator.Process<T>()
            {
                if (address.HasStatus<T>(out var tag))
                {
                    InspectorStatusElement instance = statusElementPrefab.InstantiatePrefab(container);
                    instance.Connect(address, tag);
                }
            }
        }


        [SerializeField]
        private InspectorStatusElement statusElementPrefab;

        [SerializeField]
        private Transform container;

        private void Awake()
        {
            container.ClearChildren();
        }

        public override bool Connect(InspectEntityPhase phase)
        {
            container.ClearChildren();

            CreateIconForStatus createIconForStatus = new CreateIconForStatus()
            {
                address = phase.EntityAddress,
                statusElementPrefab = statusElementPrefab,
                container = container
            };

            createIconForStatus.ForeachStatus();
            return container.childCount > 0;
        }

        public override void Disconnect(InspectEntityPhase phase)
        {
            container.ClearChildren();
        }
    }
}