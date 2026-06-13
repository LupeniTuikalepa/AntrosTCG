using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Battle
{
    public class MoveAction : EntityAction
    {
        public override int ManaCost => GameMetrics.Current.MovementCost;

        private readonly int speed;
        public MoveAction(LocalBattlePlayer player, int speed) : base(player)
        {
            this.speed = speed;
        }

        //TODO selection de case et deplacement avec une commande

        public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            await Awaitable.MainThreadAsync();
            Debug.Log("TODO : Faire les déplacements");
        }
    }
}