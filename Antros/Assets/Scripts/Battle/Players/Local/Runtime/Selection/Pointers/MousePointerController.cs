using System;
using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class MousePointerController : PointerController
    {
        private RuntimeLocalBattlePlayer runtimeLocalPlayer;

        private void Awake()
        {
            runtimeLocalPlayer = GetComponentInParent<RuntimeLocalBattlePlayer>();
        }


        public override PlayerInteractable GetPointerInteractable()
        {
            if (runtimeLocalPlayer == null)
                return null;

            PlayerControls controls = runtimeLocalPlayer.Controls;
            PlayerInteractions interactions = runtimeLocalPlayer.Interactions;

            Vector2 pointer = controls.InputModule.point.action.ReadValue<Vector2>();

            PointerEventData pointerEventData = new PointerEventData(controls.EventSystem)
            {
                position = pointer
            };
            PlayerInteractable interactable = null;

            using (ListPool<RaycastResult>.Get(out var results))
            {
                controls.EventSystem.RaycastAll(pointerEventData, results);
                RaycastResult lastResult = default;

                foreach (RaycastResult result in results)
                {
                    if (!result.gameObject.TryGetComponent(out PlayerInteractable target))
                        continue;

                    if (!interactions.IsActive(target))
                        continue;

                    if (interactable && lastResult.depth <= result.depth)
                        continue;

                    interactable = target;
                    lastResult = result;
                }
            }

            return interactable;
        }
    }
}