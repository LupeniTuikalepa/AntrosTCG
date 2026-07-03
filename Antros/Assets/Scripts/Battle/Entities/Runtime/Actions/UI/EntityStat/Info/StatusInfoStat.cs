using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.Entities.Queries;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Actions.UI.EntityStat
{
    public class StatusInfoStat : HoverStateUIElement
    {
        [Header("UI Layout")]
        [SerializeField] private GameObject mainPanel; 
        [SerializeField] private Transform statusContainer;
        [SerializeField] private GameObject statusPanelPrefab;

        private readonly List<Func<EntityAddress, bool>> statusCheckers = new List<Func<EntityAddress, bool>>();
        private readonly List<GameObject> spawnedPanel = new List<GameObject>();
        
        protected override void Awake()
        {
            base.Awake(); 
            RegisterStatusTypes(); 
        }
        private void Start()
        {
            ClearStatusIcons();
        }
        private void RegisterStatusTypes()
        {
            statusCheckers.Clear();
            RegisterStatus<PoisonStatusComponent>();
            
        }
        private void RegisterStatus<TStatus>() where TStatus : struct, IStatusComponent
        {
            statusCheckers.Add((address) =>
            {
                if (address.TryGetComponentRO<StatusInfos<TStatus>>(out var statusInfos))
                {
                    int duration = 0;
                    if (address.TryGetComponentRO<StatusDurationController<TStatus>>(out var durationController))
                    {
                        duration = durationController.RemainingTicks;
                    }
                    if (statusInfos.statusData != null)
                    {
                        SpawnPanel(statusInfos.statusData, duration);
                    }
                    return true;
                }
                return false;
            });
        }
        private void SpawnPanel(StatusData statusInfosStatusData, int duration)
        {
            if (statusPanelPrefab == null || statusContainer == null) return;

            var panelStatus = Instantiate(statusPanelPrefab, statusContainer);

            spawnedPanel.Add(panelStatus);
        }

        public override bool Build()
        {
            ClearStatusIcons();
            
            var address = EntityPhase.HoveredAddress;
            if (address == null)
            {
                if (mainPanel != null) mainPanel.SetActive(false);
                return false;
            }
            foreach (var checkStatus in statusCheckers)
            {
                checkStatus(address);
            }
            if (spawnedPanel.Count > 0)
            {
                if (mainPanel != null) mainPanel.SetActive(true);
                return true;
            }
            else
            {
                if (mainPanel != null) mainPanel.SetActive(false);
                return false;
            }
        }
        private void ClearStatusIcons()
        {
	        foreach (Transform child in statusContainer)
	        {
		        Destroy(child.gameObject);
	        }
            if (spawnedPanel == null) return;
            foreach (var icon in spawnedPanel)
            {
               if (icon != null) Destroy(icon);
            }
            spawnedPanel.Clear();
        }
        
        private void OnDisable()
        {
            ClearStatusIcons();
        }
    }
}