using System;
using System.Threading;
using ATCG.Battle.Turns;
using ATCG.Metrics;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public class LocalPlayerTurnPhase : PlayerTurnPhase
    {
        private static readonly ChannelKey channelKey = ChannelKey.GetUniqueChannelKey("Turn phase");

        public readonly LocalBattlePlayer localPlayerTurn;
        public readonly string turnID;

        //private List<IBattleActionInfos> actionInfosList;


        public LocalPlayerTurnPhase(int turnNumber, LocalBattlePlayer localPlayerTurn) : base(turnNumber,
            localPlayerTurn)
        {
            this.localPlayerTurn = localPlayerTurn;
            turnID = Guid.NewGuid().ToString();
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            //actionInfosList = ListPool<IBattleActionInfos>.Get();
            localPlayerTurn.AddOrRemoveMana(GameMetrics.Current.RecoveredManaOnTurnStart);

            localPlayerTurn.canDeployHeroes.AddCondition(channelKey);
            localPlayerTurn.canMoveHeroes.AddCondition(channelKey);
            localPlayerTurn.canUseHeroesAbilities.AddCondition(channelKey);
            localPlayerTurn.canDoBasicAttack.AddCondition(channelKey);

            localPlayerTurn.FillHand();
            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            //ListPool<IBattleActionInfos>.Release(actionInfosList);

            localPlayerTurn.canDoBasicAttack.RemoveCondition(channelKey);
            localPlayerTurn.canDeployHeroes.RemoveCondition(channelKey);
            localPlayerTurn.canMoveHeroes.RemoveCondition(channelKey);
            localPlayerTurn.canUseHeroesAbilities.RemoveCondition(channelKey);

            return base.Dispose(token);
        }

        public void DeployHero()
        {
        }

        public void EndTurn()
        {
            BattleTurn infos = new(turnID, localPlayerTurn.ID);
            SetResult(infos);
        }

        public void GiveUp()
        {
            Debug.Log("Giving up is not implemented yet");
        }
    }
}