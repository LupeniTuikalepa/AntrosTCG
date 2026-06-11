using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Battle
{
    public class MoveAction : IEntityAction
    {
        public int ManaCost => GameMetrics.Current.MovementCost;

        private readonly int speed;
        public MoveAction(int speed)
        {
            this.speed = speed;
        }

        //TODO selection de case et deplacement avec une commande

        public async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            await Awaitable.MainThreadAsync();
            Debug.Log("TODO : Faire les déplacements");
        }
    }
}