using ATCG.Battle.Players;
using ATCG.Cards.UI.Components;
using ATCG.Metrics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public abstract class PerformEntityActionUIButton<T> : EntityActionUIButton where T : EntityAction
    {
        [SerializeField]
        private ManaCostUI cost;

        public T ConnectedAction { get; private set; }


        public override bool Build()
        {
            ConnectedAction = GetActionFromPhase();
            if (ConnectedAction != null)
            {
                cost.SetCost(ConnectedAction.ManaCost);

                gameObject.SetActive(true);
                button.Interactable = RuntimeEntity.TryGetOwner(out IBattlePlayer owner) ?
                    owner.CurrentMana >= ConnectedAction.ManaCost:
                    base.IsButtonInteractable();

                return true;
            }

            return false;
        }

        protected sealed override void OnClick(BaseEventData baseEventData)
        {
            Phase.SetResult(ConnectedAction);
        }

        protected virtual T GetActionFromPhase()
        {
            if (Phase.Has(out T action))
                return action;

            return null;
        }
    }
}