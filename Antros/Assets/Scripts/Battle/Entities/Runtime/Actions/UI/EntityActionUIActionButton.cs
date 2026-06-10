using ATCG.Battle.Players;
using ATCG.Cards.UI.Components;
using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public abstract class EntityActionUIActionButton : EntityActionUIButton
    {
        [SerializeField]
        private ManaCostUI cost;

        protected override bool Build()
        {
            cost.SetCost(GetCost());

            return IsButtonCompatible();
        }

        protected override bool IsButtonInteractable() => RuntimeEntity.TryGetOwner(out var owner) ?
            owner.CurrentMana >= GetCost() :
            base.IsButtonInteractable();


        protected abstract bool IsButtonCompatible();

        protected abstract int GetCost();
    }
}