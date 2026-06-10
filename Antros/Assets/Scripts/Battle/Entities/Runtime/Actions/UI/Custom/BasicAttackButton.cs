using ATCG.Battle.Entities.Components;
using ATCG.Cards.UI.Components;
using ATCG.Metrics;
using Helteix.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.UI.Custom
{
    public class BasicAttackButton : EntityActionUIButton
    {
        [SerializeField]
        private ManaCostUI cost;

        protected override bool Build()
        {
            cost.SetCost(GameMetrics.Current.BasicAttackCost);
            if (RuntimeEntity.Address.HasComponent<BasicAttackerComponent>())
                return true;

            return false;
        }

        protected override void OnClick(BaseEventData baseEventData)
        {
            Debug.Log($"TODO : Basic attack for entity {RuntimeEntity.gameObject.name}");
            Controller.CloseAllAsync().FireAndForget();
        }
    }
}