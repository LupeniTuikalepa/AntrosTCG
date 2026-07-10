using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI.Inspector.StatusTab
{
    public class InspectorStatusList : EntityInspectorTabElement
    {
        private sealed class CreateIconForStatus : IStatusComponentIterator
        {
            public EntityAddress address;
            public InspectorStatusElement statusElementPrefab;
            public Transform container;

            public void Process<T>() where T : struct, IStatusComponent
            {
                if (address.TryGetComponentRO(out T status))
                {
                    InspectorStatusElement instance = statusElementPrefab.InstantiatePrefab(container);
                    instance.Connect(address, status);
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

            createIconForStatus.ForeachStatusComponent();
            return container.childCount > 0;
        }

        public override void Disconnect(InspectEntityPhase phase)
        {
            container.ClearChildren();
        }
    }
}