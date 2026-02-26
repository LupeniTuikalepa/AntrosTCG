using System;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Actions;
using ATCG.Battle.Actions.TurnInfos;
using ATCG.Battle.Metrics;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public class LocalPlayerTurnPhase : PlayerTurnPhase
    {
        private static ChannelKey channelKey = ChannelKey.GetUniqueChannelKey($"Turn phase");

        public readonly LocalBattlePlayer localPlayerTurn;
        public readonly string turnID;

        private List<IBattleActionInfos> actionInfosList;


        public LocalPlayerTurnPhase(int turnNumber, LocalBattlePlayer localPlayerTurn) : base(turnNumber, localPlayerTurn)
        {
            this.localPlayerTurn = localPlayerTurn;
            turnID = Guid.NewGuid().ToString();
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            actionInfosList = ListPool<IBattleActionInfos>.Get();
            localPlayerTurn.AddOrRemoveMana(GameplayMetrics.Current.RecoveredManaOnTurnStart);

            localPlayerTurn.canDeployHeroes.AddCondition(channelKey);
            localPlayerTurn.canMoveHeroes.AddCondition(channelKey);
            localPlayerTurn.canUseHeroesAbilities.AddCondition(channelKey);
            localPlayerTurn.canDoBasicAttack.AddCondition(channelKey);

            localPlayerTurn.FillHand();
            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            ListPool<IBattleActionInfos>.Release(actionInfosList);

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
            BattleTurn infos = new BattleTurn()
            {
                actions = actionInfosList.ToArray(),
                turnID = turnID,
            };

            SetResult(infos);
        }

        public void GiveUp()
        {
            Debug.Log("Giving up is not implemented yet");
            return;
        }

    }
}