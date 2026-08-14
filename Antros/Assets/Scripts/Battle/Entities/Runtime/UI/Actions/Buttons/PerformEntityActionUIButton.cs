using ATCG.Battle.Players;
using ATCG.Cards.UI.Components;
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
            bool hasAction = ConnectedAction != null;
            
            gameObject.SetActive(hasAction);
            if (hasAction)
            {
                cost.SetCost(ConnectedAction.ManaCost);
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