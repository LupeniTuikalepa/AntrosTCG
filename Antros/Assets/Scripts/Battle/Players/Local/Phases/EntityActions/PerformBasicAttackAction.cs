using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Battle
{
    public class PerformBasicAttackAction : IEntityAction
    {
        private readonly int strength;
        public int ManaCost => GameMetrics.Current.BasicAttackCost;

        public PerformBasicAttackAction(int strength)
        {
            this.strength = strength;
        }

        //TODO selection de case et deplacement avec une commande
        public async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            await Awaitable.MainThreadAsync();
            Debug.Log("TODO : Faire les attaques physiques");
        }
    }
}