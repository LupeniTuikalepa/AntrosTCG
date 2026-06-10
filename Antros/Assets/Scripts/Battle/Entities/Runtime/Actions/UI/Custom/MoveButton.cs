using ATCG.Battle.Entities.Components;
using ATCG.Cards.UI.Components;
using ATCG.Metrics;
using Helteix.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.UI.Custom
{
    public class MoveButton : EntityActionUIActionButton
    {
        protected override bool IsButtonCompatible() => RuntimeEntity.Address.HasComponent<MovementComponent>();

        protected override int GetCost() => GameMetrics.Current.MovementCost;

        protected override void OnClick(BaseEventData baseEventData)
        {
            Debug.Log($"TODO : Movement for entity {RuntimeEntity.gameObject.name}");
            Controller.CloseAllAsync().FireAndForget();
        }
    }
}