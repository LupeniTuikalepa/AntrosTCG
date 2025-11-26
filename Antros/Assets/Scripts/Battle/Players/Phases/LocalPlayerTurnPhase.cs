using System;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Actions;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players.Local;
using Helteix.ChanneledProperties;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players
{
    public class LocalPlayerTurnPhase : PhaseCompletionSource<BattleTurn>
    {
        private static ChannelKey channelKey = ChannelKey.GetUniqueChannelKey($"Turn phase");

        public readonly int turnNumber;
        public readonly string turnID;

        public readonly LocalBattlePlayer battlePlayer;

        private List<IBattleActionInfos> actionInfosList;

        private AwaitableCompletionSource<BattleTurn> completionSource;

        public LocalPlayerTurnPhase(int turnNumber, LocalBattlePlayer battlePlayer)
        {
            this.turnNumber = turnNumber;
            this.battlePlayer = battlePlayer;
            turnID = Guid.NewGuid().ToString();
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            actionInfosList = ListPool<IBattleActionInfos>.Get();

            battlePlayer.AddOrRemoveMana(GameplayMetrics.Current.RecoveredManaOnTurnStart);

            battlePlayer.canDeployHeroes.AddCondition(channelKey);
            battlePlayer.canMoveHeroes.AddCondition(channelKey);
            battlePlayer.canUseHeroesAbilities.AddCondition(channelKey);

            return base.Initialize(token);
        }
        protected override Awaitable Dispose(CancellationToken token)
        {
            ListPool<IBattleActionInfos>.Release(actionInfosList);

            battlePlayer.canDeployHeroes.RemoveCondition(channelKey);
            battlePlayer.canMoveHeroes.RemoveCondition(channelKey);
            battlePlayer.canUseHeroesAbilities.RemoveCondition(channelKey);

            return base.Dispose(token);
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