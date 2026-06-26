using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Components.Status.Signals;
using ATCG.Capacities.Data.Status;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatusController : MonoBehaviour, IEntityCommandListener<StatusSignal>
    {
        public Entity Entity => runtimeEntity.Address.entity;
        
        [SerializeField]
        private Transform statusRoot;
        
        private IRuntimeEntity runtimeEntity;
        private Dictionary<StatusData, RuntimeStatus> statusDatas;

        private void Awake()
        {
            runtimeEntity = GetComponentInParent<IRuntimeEntity>();
            statusDatas = new();
            if (statusRoot == null)
                statusRoot = transform;
        }

        private void OnEnable()
        {
            this.RegisterListener();
        }

        private void OnDisable()
        {
            this.UnregisterListener();
        }

        public async Awaitable Play(CommandListenerState state, CommandContext context, StatusSignal command)
        {
            await Awaitable.MainThreadAsync();
            state.CompleteAll(this);
            
            var infos = command.GetInfos();
            var runtimeContext = new RuntimeStatusContext(infos.data, Entity, runtimeEntity);
            
            switch (infos.action)
            {
                case StatusAction.Apply:
                    Debug.Log("[RuntimeStatusController] Apply");
                    ApplyRuntimeStatus(runtimeContext);
                    return;
                
                case StatusAction.Remove:
                    Debug.Log("[RuntimeStatusController] Remove");
                    RemoveRuntimeStatus(runtimeContext);
                    return;
                
                case StatusAction.Tick:
                    Debug.Log("[RuntimeStatusController] Tick");
                    TickRuntimeStatus(runtimeContext);
                    return;
            }
        }

        private void TickRuntimeStatus(RuntimeStatusContext runtimeContext)
        {
            var statusData = runtimeContext.statusData;
            if (statusDatas.TryGetValue(statusData, out RuntimeStatus tickStatus))
                tickStatus.Tick(runtimeContext);
        }

        private void RemoveRuntimeStatus(RuntimeStatusContext runtimeContext)
        {
            var statusData = runtimeContext.statusData;
            if (!statusDatas.TryGetValue(statusData, out RuntimeStatus removeStatus)) 
                return;
            
            removeStatus.Remove();
            Destroy(removeStatus.gameObject);
            statusDatas.Remove(statusData);
        }

        private void ApplyRuntimeStatus(RuntimeStatusContext runtimeContext)
        {
            var statusData = runtimeContext.statusData;
            if (statusDatas.ContainsKey(statusData))
                return;

            if (!statusData.StatusVFX.TryGetComponent(out RuntimeStatus prefabStatus))
            {
                Debug.LogWarning($"[RuntimeStatusController] No RuntimeStatus found");
                return;
            }
            
            RuntimeStatus runtimeStatus = Instantiate(prefabStatus, transform);
            statusDatas.Add(statusData, runtimeStatus);
            runtimeStatus.Apply();
        }
    }
}
