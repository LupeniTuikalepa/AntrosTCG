using System;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Actions;
using ATCG.Battle.Metrics;
using Helteix.ChanneledProperties;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players
{
    public class PlayTurnPhase : IPhase<BattleTurn>
    {
        private static ChannelKey channelKey = ChannelKey.GetUniqueChannelKey($"Turn phase");

        public readonly int turnNumber;
        public readonly string turnID;

        public readonly LocalBattlePlayer battlePlayer;

        private List<IBattleActionInfos> actionInfosList;

        private AwaitableCompletionSource<BattleTurn> completionSource;


        public PlayTurnPhase(int turnNumber, LocalBattlePlayer battlePlayer)
        {
            this.turnNumber = turnNumber;
            this.battlePlayer = battlePlayer;
            turnID = Guid.NewGuid().ToString();
        }

        public Awaitable Initialize(CancellationToken token)
        {
            actionInfosList = ListPool<IBattleActionInfos>.Get();

            battlePlayer.AddOrRemoveMana(GameplayMetrics.Current.RecoveredManaOnTurnStart);

            battlePlayer.canDeployHeroes.AddCondition(channelKey, true);
            battlePlayer.canDiscardCards.AddCondition(channelKey, true);
            battlePlayer.canMoveHeroes.AddCondition(channelKey, true);
            battlePlayer.canUseHeroesAbilities.AddCondition(channelKey, true);

            return Awaitables.CompletedAwaitable;
        }

        public Awaitable Dispose(CancellationToken token)
        {
            ListPool<IBattleActionInfos>.Release(actionInfosList);

            battlePlayer.canDeployHeroes.RemoveCondition(channelKey);
            battlePlayer.canDiscardCards.RemoveCondition(channelKey);
            battlePlayer.canMoveHeroes.RemoveCondition(channelKey);
            battlePlayer.canUseHeroesAbilities.RemoveCondition(channelKey);

            return Awaitables.CompletedAwaitable;
        }

        public async Awaitable<BattleTurn> Execute(CancellationToken token)
        {
            return await completionSource.Awaitable;
        }

        public void DeployHero()
        {
            
        }

        public void ValidatePhase()
        {
            BattleTurn infos = new BattleTurn()
            {
                actions = actionInfosList.ToArray(),
                turnID = turnID,
            };

        }
    }
}