using System;
using System.Collections.Generic;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Heroes.Runtime;
using ATCG.Battle.Players.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class PlayerInspector : PlayerHUDElement
    {
        [SerializeField]
        private EventSystem eventSystem;

        private PointerEventData pointerEventData;

        public RuntimeHero HoveredHero { get; private set; }

        private void Awake()
        {
            pointerEventData = new PointerEventData(eventSystem);
        }

        protected override void OnConnect()
        {
        }

        protected override void OnDisconnect()
        {

        }

        private void Update()
        {
            Vector2 mousePosition = eventSystem.currentInputModule.input.mousePosition;
            pointerEventData.position = mousePosition;
            using (ListPool<RaycastResult>.Get(out List<RaycastResult> raycastsResults))
            {
                eventSystem.RaycastAll(pointerEventData, raycastsResults);
                foreach (RaycastResult result in raycastsResults)
                {
                    if (result.gameObject.CompareTag("Hero") && result.gameObject.TryGetComponent(out RuntimeHero hero))
                    {

                    }
                }
            }
        }
    }
}